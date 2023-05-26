Nuget.org publishing flow

0. Wait for dependencies to publish .... either to match version or to wait to build
1. Nugetize
2. Merge to main
3. Release (means push to nuget.org)
4. Denugetize develop
(repeat)



*******************************************

## Local build publishing flow

### Pre-req
1. Windows dev box or VM
2. Git command line
3. Developer tools
4. Ensure build machine is pointing to a known local nuget folder
5. Manifest

### Steps
1. Clone repo (git command line)
2. Switch to develop branch (git command line)
3. Create a new versioned branch off of develop (git command line)
4. Remove external references (Mirid - ToDo)
5. Switch to nuget references (Mirid)
6. Build nugets (MSBuild) - need to build and figure out where the packages are 
7. Copy nugets to local nuget repo folder (copy in command line)
--------------------------------------------------------------------------------------------
8. Push new versioned branch to GitHub (git command line)
9. PR from versioned branch to main (git command line)
10. Tag release (including nightlies) (manual on GitHub ... maybe Octokit API)

11. bulk upload all packages to nuget.org (custom script or tooling - to investigate)
--------------------------------------------------------------------------------------------

### To decide
1. What runs the app
2. How do we set the version? (command line param?)
3. How do we define the scope of publishing and the order - e.g. a manifest (markdown, or json, something .... )




1. Release Meadow Units (trigger on main using a new release in GitHub)
 	- merge to main
	- create a new release on GitHub and publish (kicks off workflow to publish nuget)

2. Release Meadow Logging (if needed) 
	- merge to main
	- create a new release on GitHub and publish (kicks off workflow to publish nuget)

MQTT (if needed) ... or delete

3. Release Meadow Contracts 
	- wait for Units and Logging nugets to publish
	- nugetize develop with wildcard versions
	- push to GitHub
	- merge to main
	- create a new release on GitHub and publish (kicks off workflow to publish nuget)
	- change develop back to local refs

Release ModBus!!!

4. Release Meadow.Core
	- wait for Contracts nuget to publish
	- nugetize Meadow.Core
	- merge develop to main
	- create a new release on GitHub and publish
	
5. Release Meadow.F7
	- wait for Meadow Core nuget to publish
	- nugetize Meadow.F7
	- update build scripts to latest release version
	- merge develop to main
	- run Meadow F7 Nuget package creation action from GitHub on main
	- change develop back to local refs (Core / F7)

5b. Release Meadow.Windows
5c. Release Meadow.Simulation
5d. Release Meadow.Linux

6. Release Meadow.Foundation.Core
	- wait for Meadow.Contracts to publish
	- nugetize Meadow.Foundation.Core
	- merge develop to main
	- create a new release on GitHub and publish

7. Release Meadow.Foundation peripherals / libraries
	- update build scripts level 1 & 2 to include any new or change packages (see Mirid tooling)
	- update build scripts level 1 & 2 to publish using latest release version (e.g. 0.94.0)
	- nugetize Meadow.Foundation peripheral driver and libs 
	- and nugetize Meadow.Foundation samples (f7 only ... leave local refs to drivers)
	- wait for M.F. Core to publish
	- run level 1 action on main
	- wait for level 1 nuget packages to publish
	- run level 2 action on main
	- change develop to local refs

8. Release Meadow.Foundation.Grove
- wait for Meadow.Foundation packages to publish

9. Release Meadow.Foundation.mikroBus
- wait for Meadow.Foundation packages to publish

10. Release Meadow.Foundation.FeatherWings
- wait for Meadow.Foundation packages to publish

11. Release ProjectLab
Jeugo
Clima
GNSS Tracker

12. Update Samples (nugetize and push to main)
- Update Meadow.Core.Samples 
- Update Meadow.Project.Samples
- Update ProjectLab.Samples

13. Release new CLI

14. Release new SDK

15. Release VS on Windows extensions

16. Release VS for Mac extensions

17. Release VSCode extension