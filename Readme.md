# For Developers only

This is a repo for the rebuild of Jackett using new cross platform technologies.

## Install instructions

Note that VS comes with outdated node and git so make sure you do all the steps.  This can be developed without visual studio as there is support for other editors.  Do not open the project or solution until instructed.

1. [Install visual studio community 2015 update 1](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx)
2. [Install the latest node (5.5+)](https://nodejs.org/en/)
3. Install the correct coreclr runtimes

``` 
dnvm install 1.0.0-rc1-update1 -r coreclr
dnvm use 1.0.0-rc1-update1 -r coreclr -p
```
4. Install [Git 2.7+](http://git-scm.com/)
5. Checkout the project towards the root of your drive c:\Git\Jackett2 or similar as node has long paths!
6. Setup node
```
cd to src\Jackett2
npm install -g jspm
npm install -g gulp
npm install -g bower
jspm install -y
```
7. Update the web tooling in Visual studio by going to Tools -> Options -> Projects and solutions -> External web tools
```
Remove all the options apart from $(Path) then add

C:\Program Files\Git\bin
C:\Program Files (x86)\nodejs
```
8. Open the Jackett.sln in visual studio and wait for it to restore the Packages.
9. Select debug profile web and try the app out.