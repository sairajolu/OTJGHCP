# Audit IP Retention Decision

For this assessment, actor IP addresses are retained for 90 days from audit-entry creation, unless a documented legal hold or security investigation requires a longer period. After the retention period, the IP value must be purged or irreversibly anonymized by a controlled data-retention process outside the append-only audit API; the audit event and non-IP fields remain immutable.

Raw IP access is restricted to explicitly authorized audit or security administrators. Ordinary audit responses, notifications, and application logs do not expose the raw value. Storage and backups must use the deployment environment's encryption controls.

This is an engineering decision for the assessment, not legal advice. Production deployment must confirm the period and any legal-hold exceptions with the applicable privacy and compliance owners.