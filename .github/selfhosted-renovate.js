let branchPrefix = "selfhosted-renovate/";

module.exports = {
  autodiscover: true,
  branchPrefix: branchPrefix,
  dependencyDashboardTitle: "Dependency Dashboard self-hosted",
  dryRun: "full",
  gitAuthor: "Renovate Bot <bot@renovateapp.com>",
  onboarding: true,
  onboardingBranch: branchPrefix + "configure",
  platform: "github",
};
