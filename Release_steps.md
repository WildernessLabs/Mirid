1. Release Meadow Units (trigger on main using a new release in GitHub)
 	- merge to main
	- create a new release on GitHub and publish (kicks off workflow to publish nuget)

2. Release Meadow Logging (if needed) 
	- nugetize
	- merge to main
	- create a new release on GitHub and publish (kicks off workflow to publish nuget)
	- switch to local refs

3. Release Meadow Contracts 
	- wait for Units and Logging nugets to publish
	- nugetize develop with wildcard versions
	- push to GitHub
	- merge to main
	- create a new release on GitHub and publish (kicks off workflow to publish nuget)
	- change develop back to local refs

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

6. Release Meadow.Foundation.Core
	- wait for Meadow.Contracts to publish
	- nugetize Meadow.Foundation.Core
	- merge develop to main
	- create a new release on GitHub and publish

7. Release Meadow.Foundation peripherals / libraries
	- nugetize Meadow.Foundation peripheral driver and libs 
	- and nugetize Meadow.Foundation samples (f7 only ... leave local refs to drivers)
	- update build scripts level 1 & 2 to include any new or change pacakges (see Mirid tooling)
	- update build scripts level 1 & 2 to publish using latest release version (e.g. 0.94.0)
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