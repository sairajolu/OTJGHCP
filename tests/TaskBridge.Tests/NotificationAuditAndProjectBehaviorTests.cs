using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskBridge.Api.Controllers;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Exceptions;
using TaskBridge.Application.Notifications;
using TaskBridge.Application.Projects;
using TaskBridge.Domain.Entities;
using TaskBridge.Domain.Enums;

namespace TaskBridge.Tests;

public sealed class NotificationAuditAndProjectBehaviorTests
{
    private static readonly Guid OrganisationOne = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OrganisationTwo = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ActorUser = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid RecipientOne = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid RecipientTwo = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid OtherUser = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid ProjectId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid TeamId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private static readonly Guid MilestoneId = Guid.Parse("12121212-1212-1212-1212-121212121212");
    private static readonly DateTime BaseTime = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Milestone_state_change_dispatches_notifications_to_every_relevant_recipient()
    {
        var auditRepository = new InMemoryAuditEntryRepository();
        var notificationRepository = new InMemoryNotificationRepository();
        var context = new TestUserContext(OrganisationOne, ActorUser);
        var handler = CreateHandler(auditRepository, notificationRepository, context);
        var milestoneEvent = CreateEvent(MilestoneEventType.MilestoneUpdated);

        await handler.HandleAsync(milestoneEvent, CancellationToken.None);

        notificationRepository.Notifications.Select(notification => notification.RecipientUserId)
            .Should().BeEquivalentTo([RecipientOne, RecipientTwo]);
        auditRepository.Entries.Should().ContainSingle(entry => entry.EventType == MilestoneEventType.MilestoneUpdated);
    }

    [Fact]
    public async Task Milestone_update_creates_an_audit_entry_with_previous_and_new_state()
    {
        var auditRepository = new InMemoryAuditEntryRepository();
        var notificationRepository = new InMemoryNotificationRepository();
        var context = new TestUserContext(OrganisationOne, ActorUser);
        var handler = CreateHandler(auditRepository, notificationRepository, context);
        var milestoneEvent = CreateEvent(
            MilestoneEventType.MilestoneUpdated,
            "{\"status\":\"Open\"}",
            "{\"status\":\"Completed\"}");

        await handler.HandleAsync(milestoneEvent, CancellationToken.None);

        var entry = auditRepository.Entries.Should().ContainSingle().Subject;
        entry.OrganisationId.Should().Be(OrganisationOne);
        entry.ActorUserId.Should().Be(ActorUser);
        entry.ProjectId.Should().Be(ProjectId);
        entry.PreviousState.Should().Be("{\"status\":\"Open\"}");
        entry.NewState.Should().Be("{\"status\":\"Completed\"}");
        entry.TimestampUtc.Should().Be(BaseTime);
    }

    [Fact]
    public async Task Reopened_milestone_creates_audit_and_notification_records()
    {
        var auditRepository = new InMemoryAuditEntryRepository();
        var notificationRepository = new InMemoryNotificationRepository();
        var context = new TestUserContext(OrganisationOne, ActorUser, "203.0.113.10");
        var handler = CreateHandler(auditRepository, notificationRepository, context);

        await handler.HandleAsync(CreateEvent(MilestoneEventType.MilestoneReopened), CancellationToken.None);

        auditRepository.Entries.Should().ContainSingle(entry => entry.EventType == MilestoneEventType.MilestoneReopened);
        notificationRepository.Notifications.Should().OnlyContain(notification => notification.EventType == MilestoneEventType.MilestoneReopened);
    }

    [Fact]
    public async Task Audit_entry_captures_the_trusted_actor_ip_address()
    {
        var repository = new InMemoryAuditEntryRepository();
        var context = new TestUserContext(OrganisationOne, ActorUser, "2001:db8::10");
        var service = new AuditService(repository, context, new TestClock(BaseTime));

        await service.AppendAsync(
            new AuditEntryRequest(
                Guid.NewGuid(),
                MilestoneEventType.MilestoneUpdated,
                "Milestone",
                MilestoneId,
                ProjectId,
                MilestoneId,
                "{\"status\":\"Open\"}",
                "{\"status\":\"Completed\"}",
                "correlation"),
            CancellationToken.None);

        repository.Commit();
        repository.Entries.Should().ContainSingle().Which.ActorIpAddress.Should().Be("2001:db8::10");
    }

    [Fact]
    public async Task Existing_audit_row_without_an_ip_remains_readable()
    {
        var repository = new InMemoryAuditEntryRepository();
        repository.Entries.Add(CreateAuditEntry(Guid.NewGuid(), BaseTime, MilestoneEventType.MilestoneUpdated));
        var service = new AuditService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        var result = await service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, null, null, null, 1, 50),
            CancellationToken.None);

        result.Should().ContainSingle();
        repository.Entries[0].ActorIpAddress.Should().BeNull();
    }

    [Fact]
    public async Task Audit_entries_cannot_be_overwritten_or_deleted_through_the_repository_contract()
    {
        typeof(IAuditEntryRepository).GetMethods()
            .Select(method => method.Name)
            .Should().NotContain("UpdateAsync");
        typeof(IAuditEntryRepository).GetMethods()
            .Select(method => method.Name)
            .Should().NotContain("DeleteAsync");

        var repository = new InMemoryAuditEntryRepository();
        var context = new TestUserContext(OrganisationOne, ActorUser);
        var service = new AuditService(repository, context, new TestClock(BaseTime));
        var request = new AuditEntryRequest(
            Guid.Parse("13131313-1313-1313-1313-131313131313"),
            MilestoneEventType.MilestoneUpdated,
            "Milestone",
            MilestoneId,
            ProjectId,
            MilestoneId,
            "{\"status\":\"Open\"}",
            "{\"status\":\"Completed\"}",
            "correlation-1");

        var first = await service.AppendAsync(request, CancellationToken.None);
        repository.Commit();
        var second = await service.AppendAsync(request with { NewState = "{\"status\":\"Archived\"}" }, CancellationToken.None);

        repository.Entries.Should().ContainSingle();
        repository.Entries[0].NewState.Should().Be("{\"status\":\"Completed\"}");
    }

    [Fact]
    public async Task Audit_history_is_filtered_by_date_range()
    {
        var repository = new InMemoryAuditEntryRepository();
        repository.Entries.AddRange(
        [
            CreateAuditEntry(Guid.NewGuid(), BaseTime.AddDays(-2), MilestoneEventType.MilestoneCreated),
            CreateAuditEntry(Guid.NewGuid(), BaseTime, MilestoneEventType.MilestoneUpdated),
            CreateAuditEntry(Guid.NewGuid(), BaseTime.AddDays(2), MilestoneEventType.MilestoneDeleted)
        ]);
        var service = new AuditService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        var result = await service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, BaseTime.AddDays(-1), BaseTime.AddDays(1), null, 1, 50),
            CancellationToken.None);

        result.Should().ContainSingle(entry => entry.EventType == MilestoneEventType.MilestoneUpdated);
    }

    [Fact]
    public async Task Audit_history_event_type_filter_returns_only_matching_entries()
    {
        var repository = new InMemoryAuditEntryRepository();
        repository.Entries.AddRange(
        [
            CreateAuditEntry(Guid.NewGuid(), BaseTime, MilestoneEventType.MilestoneCreated),
            CreateAuditEntry(Guid.NewGuid(), BaseTime.AddMinutes(1), MilestoneEventType.MilestoneReopened)
        ]);
        var service = new AuditService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        var result = await service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, null, null, MilestoneEventType.MilestoneReopened, 1, 50),
            CancellationToken.None);

        result.Should().ContainSingle(entry => entry.EventType == MilestoneEventType.MilestoneReopened);
    }

    [Fact]
    public async Task Audit_query_for_another_organisation_does_not_return_entries()
    {
        var repository = new InMemoryAuditEntryRepository();
        repository.Entries.Add(CreateAuditEntry(Guid.NewGuid(), BaseTime, MilestoneEventType.MilestoneUpdated, OrganisationTwo));
        var service = new AuditService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        var result = await service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, null, null, null, 1, 50),
            CancellationToken.None);

        result.Should().BeEmpty();
        repository.LastOrganisationId.Should().Be(OrganisationOne);
    }

    [Fact]
    public async Task User_cannot_mark_another_users_notification_as_read()
    {
        var repository = new InMemoryNotificationRepository();
        var notification = Notification.Create(
            Guid.NewGuid(),
            OtherUser,
            OrganisationOne,
            MilestoneEventType.MilestoneUpdated,
            ProjectId,
            MilestoneId,
            "Updated",
            "Updated",
            BaseTime);
        repository.Notifications.Add(notification);
        var service = new NotificationService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        Func<Task> action = () => service.MarkAsReadAsync(notification.Id, CancellationToken.None);

        await action.Should().ThrowAsync<NotificationNotFoundException>();
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Invalid_audit_date_range_is_rejected()
    {
        var service = new AuditService(
            new InMemoryAuditEntryRepository(),
            new TestUserContext(OrganisationOne, ActorUser),
            new TestClock(BaseTime));

        Func<Task> action = () => service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, BaseTime.AddDays(1), BaseTime, null, 1, 50),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidAuditDateRangeException>();
    }

    [Fact]
    public async Task Audit_date_range_over_90_days_is_rejected()
    {
        var service = new AuditService(
            new InMemoryAuditEntryRepository(),
            new TestUserContext(OrganisationOne, ActorUser),
            new TestClock(BaseTime));

        Func<Task> action = () => service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, BaseTime.AddDays(-91), BaseTime, null, 1, 50),
            CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Audit_response_does_not_expose_raw_snapshots()
    {
        typeof(AuditEntryResponse).GetProperty(nameof(AuditEntry.PreviousState)).Should().BeNull();
        typeof(AuditEntryResponse).GetProperty(nameof(AuditEntry.NewState)).Should().BeNull();
        typeof(AuditEntryResponse).GetProperty(nameof(AuditEntry.ActorIpAddress)).Should().BeNull();
    }

    [Fact]
    public async Task Audit_controller_does_not_return_exception_details()
    {
        var controller = new AuditController(new ThrowingAuditService());

        var result = await controller.GetByProjectAsync(
            ProjectId,
            new AuditQueryParameters(),
            CancellationToken.None);

        var response = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problem = response.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Be("The audit query is invalid.");
        problem.Detail.Should().NotContain("internal query details");
    }

    [Fact]
    public async Task Invalid_project_status_transition_is_rejected()
    {
        var project = Project.Create(OrganisationOne, TeamId, "Project", null, BaseTime);
        var repository = new InMemoryProjectRepository(project);
        var service = new ProjectService(
            repository,
            new TestUserContext(OrganisationOne, ActorUser),
            new TestClock(BaseTime),
            new CreateProjectRequestValidator(),
            new UpdateProjectStatusRequestValidator());

        Func<Task> action = () => service.UpdateStatusAsync(
            new UpdateProjectStatusRequest(project.Id, ProjectStatus.Completed),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidProjectStatusTransitionException>();
        repository.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Tenant_scoped_project_lookup_does_not_disclose_another_tenants_project()
    {
        var project = Project.Create(OrganisationTwo, TeamId, "Other tenant project", null, BaseTime);
        var repository = new InMemoryProjectRepository(project);
        var service = new ProjectService(
            repository,
            new TestUserContext(OrganisationOne, ActorUser),
            new TestClock(BaseTime),
            new CreateProjectRequestValidator(),
            new UpdateProjectStatusRequestValidator());

        Func<Task> action = () => service.UpdateStatusAsync(
            new UpdateProjectStatusRequest(project.Id, ProjectStatus.Active),
            CancellationToken.None);

        await action.Should().ThrowAsync<ProjectNotFoundException>();
        project.Status.Should().Be(ProjectStatus.Draft);
    }

    [Fact]
    public async Task Notification_query_returns_unread_notifications_only()
    {
        var repository = new InMemoryNotificationRepository();
        var unread = Notification.Create(Guid.NewGuid(), ActorUser, OrganisationOne, MilestoneEventType.MilestoneUpdated, ProjectId, MilestoneId, "Unread", "Unread", BaseTime);
        var read = Notification.Create(Guid.NewGuid(), ActorUser, OrganisationOne, MilestoneEventType.MilestoneUpdated, ProjectId, MilestoneId, "Read", "Read", BaseTime);
        read.MarkAsRead(BaseTime.AddMinutes(1));
        repository.Notifications.AddRange([unread, read]);
        var service = new NotificationService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        var result = await service.GetForUserAsync(ActorUser, new NotificationQuery(1, 50), CancellationToken.None);

        result.Should().ContainSingle(notification => notification.Id == unread.Id);
    }

    [Fact]
    public async Task CancellationToken_is_propagated_to_audit_query()
    {
        using var cancellationSource = new CancellationTokenSource();
        var repository = new InMemoryAuditEntryRepository();
        var service = new AuditService(repository, new TestUserContext(OrganisationOne, ActorUser), new TestClock(BaseTime));

        await service.GetByProjectAsync(
            new AuditEntryQuery(ProjectId, null, null, null, 1, 50),
            cancellationSource.Token);

        repository.LastCancellationToken.Should().Be(cancellationSource.Token);
    }

    private static MilestoneEventHandler CreateHandler(
        InMemoryAuditEntryRepository auditRepository,
        InMemoryNotificationRepository notificationRepository,
        TestUserContext context)
    {
        return new MilestoneEventHandler(
            new AuditService(auditRepository, context, new TestClock(BaseTime)),
            new NotificationService(notificationRepository, context, new TestClock(BaseTime)),
            new TestRecipientResolver(RecipientOne, RecipientTwo),
            context,
            new TestUnitOfWork(auditRepository, notificationRepository));
    }

    private static MilestoneChangedEvent CreateEvent(
        MilestoneEventType eventType,
        string? previousState = "{\"status\":\"Open\"}",
        string? newState = "{\"status\":\"Completed\"}")
    {
        return new MilestoneChangedEvent(
            Guid.Parse("14141414-1414-1414-1414-141414141414"),
            OrganisationOne,
            ActorUser,
            "203.0.113.10",
            ProjectId,
            TeamId,
            MilestoneId,
            eventType,
            previousState,
            newState,
            BaseTime,
            "correlation");
    }

    private static AuditEntry CreateAuditEntry(
        Guid eventId,
        DateTime timestampUtc,
        MilestoneEventType eventType,
        Guid? organisationId = null)
    {
        return AuditEntry.Create(
            eventId,
            eventType,
            "Milestone",
            MilestoneId,
            ActorUser,
            organisationId ?? OrganisationOne,
            null,
            "{\"status\":\"Open\"}",
            "{\"status\":\"Completed\"}",
            timestampUtc,
            ProjectId,
            MilestoneId,
            "correlation",
            timestampUtc);
    }

    private sealed class TestUserContext : ICurrentUserContext
    {
        public TestUserContext(Guid organisationId, Guid userId, string? actorIpAddress = null)
        {
            OrganisationId = organisationId;
            UserId = userId;
            ActorIpAddress = actorIpAddress;
        }

        public Guid OrganisationId { get; }
        public Guid UserId { get; }
        public string? ActorIpAddress { get; }
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }

    private sealed class TestRecipientResolver : IMilestoneRecipientResolver
    {
        private readonly IReadOnlyCollection<Guid> recipients;

        public TestRecipientResolver(params Guid[] recipients)
        {
            this.recipients = recipients;
        }

        public Task<IReadOnlyCollection<Guid>> ResolveAsync(
            Guid organisationId,
            Guid teamId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(recipients);
        }
    }

    private sealed class TestUnitOfWork : IUnitOfWork
    {
        private readonly InMemoryAuditEntryRepository auditRepository;
        private readonly InMemoryNotificationRepository notificationRepository;

        public TestUnitOfWork(
            InMemoryAuditEntryRepository auditRepository,
            InMemoryNotificationRepository notificationRepository)
        {
            this.auditRepository = auditRepository;
            this.notificationRepository = notificationRepository;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            auditRepository.Commit();
            notificationRepository.Commit();
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryAuditEntryRepository : IAuditEntryRepository
    {
        private readonly List<AuditEntry> stagedEntries = [];

        public List<AuditEntry> Entries { get; } = [];
        public Guid LastOrganisationId { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task AddAsync(AuditEntry entry, CancellationToken cancellationToken)
        {
            stagedEntries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEventIdAsync(Guid organisationId, Guid eventId, CancellationToken cancellationToken)
        {
            LastOrganisationId = organisationId;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(Entries.Any(entry => entry.OrganisationId == organisationId && entry.EventId == eventId));
        }

        public Task<AuditEntry?> GetByEventIdAsync(Guid organisationId, Guid eventId, CancellationToken cancellationToken)
        {
            LastOrganisationId = organisationId;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(Entries.SingleOrDefault(entry => entry.OrganisationId == organisationId && entry.EventId == eventId));
        }

        public Task<IReadOnlyList<AuditEntry>> QueryAsync(Guid organisationId, AuditEntryQuery query, CancellationToken cancellationToken)
        {
            LastOrganisationId = organisationId;
            LastCancellationToken = cancellationToken;
            var result = Entries
                .Where(entry => entry.OrganisationId == organisationId && entry.ProjectId == query.ProjectId)
                .Where(entry => !query.FromUtc.HasValue || entry.TimestampUtc >= query.FromUtc.Value)
                .Where(entry => !query.ToUtc.HasValue || entry.TimestampUtc <= query.ToUtc.Value)
                .Where(entry => !query.EventType.HasValue || entry.EventType == query.EventType.Value)
                .OrderByDescending(entry => entry.TimestampUtc)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToArray();
            return Task.FromResult<IReadOnlyList<AuditEntry>>(result);
        }

        public void Commit()
        {
            Entries.AddRange(stagedEntries);
            stagedEntries.Clear();
        }
    }

    private sealed class InMemoryNotificationRepository : INotificationRepository
    {
        private readonly List<Notification> stagedNotifications = [];

        public List<Notification> Notifications { get; } = [];

        public Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            stagedNotifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByEventIdAndRecipientAsync(Guid organisationId, Guid eventId, Guid recipientUserId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Notifications.Any(notification => notification.OrganisationId == organisationId && notification.EventId == eventId && notification.RecipientUserId == recipientUserId));
        }

        public Task<IReadOnlyList<Notification>> QueryAsync(Guid organisationId, Guid recipientUserId, NotificationQuery query, CancellationToken cancellationToken)
        {
            var result = Notifications
                .Where(notification => notification.OrganisationId == organisationId && notification.RecipientUserId == recipientUserId && !notification.IsRead)
                .OrderByDescending(notification => notification.CreatedAtUtc)
                .Take(query.PageSize)
                .ToArray();
            return Task.FromResult<IReadOnlyList<Notification>>(result);
        }

        public Task<Notification?> GetForRecipientAsync(Guid organisationId, Guid recipientUserId, Guid notificationId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Notifications.SingleOrDefault(notification => notification.OrganisationId == organisationId && notification.RecipientUserId == recipientUserId && notification.Id == notificationId));
        }

        public void Commit()
        {
            Notifications.AddRange(stagedNotifications);
            stagedNotifications.Clear();
        }
    }

    private sealed class InMemoryProjectRepository : IProjectRepository
    {
        private readonly List<Project> projects;

        public InMemoryProjectRepository(Project project)
        {
            projects = [project];
        }

        public int SaveChangesCallCount { get; private set; }

        public Task AddAsync(Project project, CancellationToken cancellationToken)
        {
            projects.Add(project);
            return Task.CompletedTask;
        }

        public Task<Project?> GetByIdAsync(Guid organisationId, Guid projectId, CancellationToken cancellationToken)
        {
            return Task.FromResult(projects.SingleOrDefault(project => project.OrganisationId == organisationId && project.Id == projectId));
        }

        public Task<IReadOnlyList<Project>> GetByTeamAsync(Guid organisationId, Guid teamId, CancellationToken cancellationToken)
        {
            IReadOnlyList<Project> result = projects.Where(project => project.OrganisationId == organisationId && project.TeamId == teamId).ToArray();
            return Task.FromResult(result);
        }

        public void Remove(Project project)
        {
            projects.Remove(project);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingAuditService : IAuditService
    {
        public Task<AuditEntryResponse> AppendAsync(AuditEntryRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AuditEntryResponse> AppendForEventAsync(MilestoneChangedEvent milestoneEvent, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<AuditEntryResponse>> GetByProjectAsync(AuditEntryQuery query, CancellationToken cancellationToken)
        {
            throw new ArgumentException("internal query details", nameof(query));
        }
    }
}
