using FluentAssertions;
using FluentValidation;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Notifications;
using TaskBridge.Application.Projects;
using TaskBridge.Domain.Entities;

namespace TaskBridge.Tests;

public sealed class ProjectServiceTests
{
    private static readonly Guid OrganisationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TeamId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly DateTime CurrentTime = new(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_creates_a_draft_project_for_the_current_organisation()
    {
        var repository = new InMemoryProjectRepository();
        var service = CreateService(repository);

        var result = await service.CreateAsync(
            new CreateProjectRequest(TeamId, "  Website refresh  ", "  New content  "),
            CancellationToken.None);

        result.OrganisationId.Should().Be(OrganisationId);
        result.TeamId.Should().Be(TeamId);
        result.Name.Should().Be("Website refresh");
        result.Description.Should().Be("New content");
        result.Status.Should().Be(ProjectStatus.Draft);
        repository.LastOrganisationId.Should().Be(OrganisationId);
    }

    [Fact]
    public async Task UpdateStatusAsync_updates_a_project_in_the_current_organisation()
    {
        var repository = new InMemoryProjectRepository();
        var project = Project.Create(OrganisationId, TeamId, "Website refresh", null, CurrentTime);
        repository.Projects.Add(project);
        var service = CreateService(repository);

        var result = await service.UpdateStatusAsync(
            new UpdateProjectStatusRequest(project.Id, ProjectStatus.Active),
            CancellationToken.None);

        result.Status.Should().Be(ProjectStatus.Active);
        result.UpdatedAtUtc.Should().Be(CurrentTime);
    }

    [Fact]
    public async Task GetByTeamAsync_returns_only_projects_for_the_requested_team()
    {
        var repository = new InMemoryProjectRepository();
        repository.Projects.Add(Project.Create(OrganisationId, TeamId, "Website refresh", null, CurrentTime));
        repository.Projects.Add(Project.Create(OrganisationId, Guid.NewGuid(), "Internal tools", null, CurrentTime));
        var service = CreateService(repository);

        var result = await service.GetByTeamAsync(TeamId, CancellationToken.None);

        result.Should().ContainSingle(project => project.Name == "Website refresh");
        repository.LastOrganisationId.Should().Be(OrganisationId);
    }

    [Fact]
    public async Task DeleteAsync_removes_a_project_in_the_current_organisation()
    {
        var repository = new InMemoryProjectRepository();
        var project = Project.Create(OrganisationId, TeamId, "Website refresh", null, CurrentTime);
        repository.Projects.Add(project);
        var service = CreateService(repository);

        await service.DeleteAsync(project.Id, CancellationToken.None);

        repository.Projects.Should().BeEmpty();
    }

    private static ProjectService CreateService(InMemoryProjectRepository repository)
    {
        return new ProjectService(
            repository,
            new TestTenant(OrganisationId),
            new TestClock(CurrentTime),
            new CreateProjectRequestValidator(),
            new UpdateProjectStatusRequestValidator(),
            milestoneEventHandler: new NoOpMilestoneEventHandler());
    }

    private sealed class NoOpMilestoneEventHandler : IMilestoneEventHandler
    {
        public Task HandleAsync(
            MilestoneChangedEvent milestoneEvent,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestTenant : ICurrentTenant
    {
        public TestTenant(Guid organisationId)
        {
            OrganisationId = organisationId;
        }

        public Guid OrganisationId { get; }

        public Guid UserId { get; } = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    }

    private sealed class TestClock : IClock
    {
        public TestClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; }
    }

    private sealed class InMemoryProjectRepository : IProjectRepository
    {
        public List<Project> Projects { get; } = [];

        public Guid LastOrganisationId { get; private set; }

        public Task AddAsync(Project project, CancellationToken cancellationToken)
        {
            Projects.Add(project);
            LastOrganisationId = project.OrganisationId;
            return Task.CompletedTask;
        }

        public Task<Project?> GetByIdAsync(
            Guid organisationId,
            Guid projectId,
            CancellationToken cancellationToken)
        {
            LastOrganisationId = organisationId;
            return Task.FromResult(Projects.SingleOrDefault(
                project => project.OrganisationId == organisationId && project.Id == projectId));
        }

        public Task<IReadOnlyList<Project>> GetByTeamAsync(
            Guid organisationId,
            Guid teamId,
            CancellationToken cancellationToken)
        {
            LastOrganisationId = organisationId;
            IReadOnlyList<Project> projects = Projects
                .Where(project => project.OrganisationId == organisationId && project.TeamId == teamId)
                .ToList();
            return Task.FromResult(projects);
        }

        public void Remove(Project project)
        {
            Projects.Remove(project);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}