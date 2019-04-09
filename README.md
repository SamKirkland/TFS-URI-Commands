# TFS-URI-Commands

This project registers a URI command for tfs. Allowing you to run TFS commands from URI links.
Useful for triggering merges/commits without leaving your web browser.

##### Setup Steps
- Download the project as a zip
- Unzip the download
- Open `TFS-URI-Commands-master/CustomProtocol/bin/Release/`
- Shift right-click on `CustomProtocol.exe` and Run As Administrator
- This will install a custom URL handler for the TFS merge links
- Note: May need to setup a path for `TF` in your environment variables if you don't have it setup aleardy (reboot when done).
- You can now trigger any tfs command from a URI/URL
- Make sure you escape the url before running it (with encodeURIComponent or similar)


### Examples
###### Triggering TFS from javascript
```javascript
const startingChangeset = 1;
const endingChangeset = 123;
const sourceBranch = `"$/__FromProjectName__"`;
const destinationBranch = `"$/__ToProjectName__"`;
const mergeCommand = merge ${sourceBranch} ${destinationBranch} /version:C${endingChangeset}~C${startingChangeset} /noprompt /recursive`;
window.location.href = `tfmerge:${encodeURIComponent(mergeCommand)}`;
```


##### Security notes
When installing please edit the source and whitelist all domains you trust to run this command.
