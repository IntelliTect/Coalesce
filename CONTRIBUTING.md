Contributing
============

- [Contributing](#contributing)
  - [Prerequisites](#prerequisites)
  - [Code](#code)
    - [Code style](#code-style)
    - [Dependencies](#dependencies)
    - [Unit tests](#unit-tests)
  - [Contributing process](#contributing-process)
    - [Get buyoff or find open community issues or features](#get-buyoff-or-find-open-community-issues-or-features)
    - [Set up your environment](#set-up-your-environment)
    - [Update documentation](#update-documentation)
    - [Prepare commits](#prepare-commits)
    - [Submit pull request](#submit-pull-request)
    - [Respond to feedback on pull request](#respond-to-feedback-on-pull-request)
  - [NuGet](#nuget)
  - [Other general information](#other-general-information)
  - [Acknowledgement](#acknowledgement)

## Prerequisites

By contributing to IntelliTect, you assert that:

* The contribution is your own original work.
* You have the right to assign the copyright for the work (it is not owned by your employer, or
  you have been given copyright assignment in writing).
* You [license](license.txt) the contribution under the terms applied to the rest of the IntelliTect project.

## Code
### Code style

Normal .NET coding guidelines apply.
See the [Framework Design Guidelines](https://msdn.microsoft.com/en-us/library/ms229042%28v=vs.110%29.aspx) for more information.

### Dependencies

In general, the assemblies in this framework should have no new dependencies.

If you want to introduce a dependency to a specific library, because you are adding functionality that requires the dependency, make sure you bring it
up in an issue or a pull request, so it can be properly discussed.

### Unit tests

Make sure to run all unit tests before creating a pull request.  Also make sure that code-generation runs without errors and you have an example in the Demo project.
Any new code should also have reasonable unit test coverage.

## Contributing process
### Get buyoff or find open community issues or features

 * Through GitHub, you talk about a feature you would like to see (or a bug), and why it should be in the library.
 * Once you get a nod from one of the [Core Contributors](https://github.com/orgs/IntelliTect/teams/coalesce-core-contributors), you can start on the feature.
 * Alternatively, if a feature is on the issues list with the
   [Help Wanted](https://github.com/IntelliTect/Coalesce/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) label,
   it is open for a community member (contributor) to patch. You should comment that you are signing up for it on the issue so someone else doesn't also sign up for the work.
 * If you are new to contributing to an open-source project or Coalesce, look for issues with the [Good First Issue](https://github.com/IntelliTect/Coalesce/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) label.

### Set up your environment

 * Create a branch named specific to the feature.
 * In the branch you do work specific to the feature.
 * Please also observe the following:
    * No reformatting (resist the allure of Resharper!)
    * Do not change files that are not specific to the feature or it's documentation.
    * More covered below in the [Prepare commits](#prepare-commits) section.
 * Test your changes and please help us out by updating and implementing some automated tests.
   It is recommended that all contributors spend some time looking over the tests in the source code.
   You can't go wrong emulating one of the existing tests and then changing it specific to the behavior you are testing.
    * You may have to run `npm ci` from the [src/coalesce-vue](src/coalesce-vue) directory for some of the automated tests to run.
 * See the [debugging/environment guide](ENVIRONMENT.md) for more details.

### Update documentation
* Make sure you update or add documentation for the feature or bug you are fixing.
* The documentation is built with [VuePress 2](https://v2.vuepress.vuejs.org/). To develop locally:
  * Open the `docs` folder as a workspace in VS code.
  * `npm ci && npm run dev`. Open the URL it provides you in your browser.
  * You can now develop the documentation. Changes will be reflected in your browser as you save via HMR.

### Prepare commits
This section serves to help you understand what makes a good commit.

A commit should observe the following:

 * A commit is a small logical unit that represents a change.
 * Should include new or changed tests relevant to the changes you are making.
 * No unnecessary whitespace. Check for whitespace with `git diff --check` and `git diff --cached --check` before commit.
 * You can stage parts of a file for commit.

### Submit pull request
Prerequisites:

 * You are making commits in a feature branch.
 * All code should compile without errors or warnings.
 * All tests should be passing.
 * Code generation should run without error.

Submitting PR:

 * Once you feel it is ready, submit the pull request to the `Coalesce` repository against your feature/issue branch
   unless specifically requested to submit it against another branch.
   * In the case of a larger change that is going to require more discussion,
     please submit a PR sooner. Waiting until you are ready may mean more changes than you are
     interested in if the changes are taking things in a direction the maintainers do not want to go.
 * In the pull request, outline what you did and point to specific conversations (as in URLs or GitHub issues `#ISSUE_NUMBER`)
   and issues that you are resolving. This is a tremendous help for us in evaluation and acceptance.
 * Once the pull request is in, please do not delete the branch or close the pull request
   (unless something is wrong with it).
 * One of the Core Contributor team members, or one of the maintainers, will evaluate it within a
   reasonable time period (which is to say usually within 1-3 days). Some things get evaluated
   faster or fast-tracked.

### Respond to feedback on pull request

We may have feedback for you to fix or change some things. We generally like to see that pushed against
the same topic branch (it will automatically update the Pull Request). You can also fix/squash/rebase
commits and push the same topic branch with `--force` (it's generally acceptable to do this on topic
branches not in the main repository, it is generally unacceptable and should be avoided at all costs
against the main repository).

If we have comments or questions when we do evaluate it and receive no response, it will probably
lessen the chance of getting accepted. Eventually, this means it will be closed if it is not accepted.
Please know this doesn't mean we don't value your contribution, just that things go stale. If in the
future you want to pick it back up, feel free to address our concerns/questions/feedback and reopen
the issue/open a new PR (referencing old one).

Sometimes we may need you to rebase your commit against the latest code before we can review it further.
If this happens, you can do the following:

 * `git fetch upstream` (upstream would be the mainstream repo or `IntelliTect` in this case)
 * `git checkout master`
 * `git rebase upstream/master`
 * `git checkout your-branch`
 * `git rebase master`
 * Fix any merge conflicts
 * `git push origin your-branch` (origin would be your GitHub repo or `your-github-username/IntelliTect` in this case).
   You may need to `git push origin your-branch --force` to get the commits pushed.
   This is generally acceptable with topic branches not in the mainstream repository.

The only reasons a pull request should be closed and resubmitted are as follows:

 * When the pull request is targeting the wrong branch (this doesn't happen as often).
 * When there are updates made to the original by someone other than the original contributor.
   Then the old branch is closed with a note on the newer branch this supersedes #github_number.

## NuGet
Currently [AppVeyor](https://ci.appveyor.com/project/IntelliTect/coalesce) is building and releasing NuGets on every merge to the release branch. All PRs should be merged into master and then a PR from master into release when new NuGets are needed. Avoid PRs from feature branches directly to the release branch.

## Other general information
If you reformat code or hit core functionality without an approval from a person on the IntelliTect Team,
it's likely that no matter how awesome it looks afterwards, it will probably not get accepted.
Reformatting code makes it harder for us to evaluate exactly what was changed.

If you do these things, it will be make evaluation and acceptance easy.
Now if you stray outside of the guidelines we have above, it doesn't mean we are going to ignore
your pull request. It will just make things harder for us.
Harder for us roughly translates to a longer SLA for your pull request.

## Acknowledgement

This contribution guide was inspired by the [Cake Project](https://github.com/cake-build/cake#contributing)'s contribution guide.
