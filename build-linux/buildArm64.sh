#!/bin/bash

# Version
buildVersion=$(<../build-resources/version.txt)

# Clean-up
rm -rf ./staging_folder_arm64/

# .NET publish
# self-contained is recommended, so final users won't need to install .NET
dotnet publish ../UnityBuilder.Desktop/UnityBuilder.Desktop.csproj --configuration Release --runtime linux-arm64 --self-contained -f net9.0
echo "Published"

# Staging directory
mkdir -p staging_folder_arm64

# Debian control file
mkdir -p ./staging_folder_arm64/DEBIAN
cp ./linux-data-arm64/control ./staging_folder_arm64/DEBIAN
sed -i "s/currentVersionIsPlacedHere/${buildVersion}/g" ./staging_folder_arm64/DEBIAN/control
echo "Control copied"

# Starter script
mkdir -p ./staging_folder_arm64/usr
mkdir -p ./staging_folder_arm64/usr/bin
cp ./linux-data-arm64/unity-builder ./staging_folder_arm64/usr/bin/unity-builder
chmod +x ./staging_folder_arm64/usr/bin/unity-builder # set executable permissions to starter script
echo "Started copied"

# Other files
mkdir -p ./staging_folder_arm64/usr/share
mkdir -p ./staging_folder_arm64/usr/share/unity-builder
cp -f -a ../UnityBuilder.Desktop/bin/Release/net9.0/linux-arm64/publish/. ./staging_folder_arm64/usr/share/unity-builder/ # copies all files from publish dir
chmod -R a+rX ./staging_folder_arm64/usr/share/unity-builder/ # set read permissions to all files
chmod a+x ./staging_folder_arm64/usr/share/unity-builder/UnityBuilder.Desktop # set executable permissions to main executable
echo "UnityBuilder copied"

# Default execute script
cp -f -r ../build-resources/default-build-script ./staging_folder_arm64/usr/share/unity-builder/ # copies all files from publish dir
echo "Default execute script copied"

# Desktop shortcut
mkdir -p ./staging_folder_arm64/usr/share/applications
cp ./linux-data-arm64/UnityBuilder.desktop ./staging_folder_arm64/usr/share/applications/UnityBuilder.desktop
echo "Shortcut copied"

# Stables
cp ../build-resources/computed_stables.txt ./staging_folder_arm64/usr/share/unity-builder/computed_stables.txt
echo "Stables copied"

# Desktop icon
# A 1024px x 1024px PNG, like VS Code uses for its icon
mkdir -p ./staging_folder_arm64/usr/share/pixmaps
cp ../build-resources/logo.png ./staging_folder_arm64/usr/share/pixmaps/unity-builder.png
echo "Icon copied"

# Hicolor icons
mkdir -p ./staging_folder_arm64/usr/share/icons
mkdir -p ./staging_folder_arm64/usr/share/icons/hicolor
mkdir -p ./staging_folder_arm64/usr/share/icons/hicolor/scalable
mkdir -p ./staging_folder_arm64/usr/share/icons/hicolor/scalable/apps
cp ../build-resources/logo.svg ./staging_folder_arm64/usr/share/icons/hicolor/scalable/apps/unity-builder.svg
echo "Another icon copied"

# Make .deb file
dpkg-deb --root-owner-group --build ./staging_folder_arm64/ ./unity-builder-arm64.deb
echo ".deb created"
