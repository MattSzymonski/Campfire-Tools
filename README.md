# Campfire Tools
C# utilities library

## Contents
- Web scrapping utilities
- Google Sheets utilities
- Open AI utilities

## Usage in C# projects
To use this library in a C# project, add one of the following snippets to the `.csproj` file:

### Reference the library (recommended)
This requires DLL to be rebuild each time a change in the library is introduced.
```
<Reference Include="CampfireTools">
  <HintPath>libs\CampfireTools.dll</HintPath>
</Reference>
```

### Reference the project (not recommended)
Not recommended for Docker programs due to problems with relative reference paths.
This requires to have access to the project files.

```
<ItemGroup>
  <ProjectReference Include="<CAMPFIRE-TOOLS_PROJECT_LOCATION>\CampfireTools.csproj" />
</ItemGroup>
```


## Usage in C# docker projects
Depending on the project structure, the library can be linked in two ways: DLL or project reference.

1. If library is referenced as DLL in /libs directory then either:
  2. Have DLL built in /libs folder in the source (this requires rebuilding it in before creating Docker image)
  3. Have library's source code and build it there (this requires copying its source code to Docker image)
```
# --- BUILD LIBRARIES ---
ARG CAMPFIRE_TOOLS_DIR_PATH=../../campfire-tools/

WORKDIR /libs/campfire-tools
COPY ${CAMPFIRE_TOOLS_DIR_PATH} .
RUN dotnet restore "CampfireTools.csproj"
RUN dotnet build -o ./output
COPY ./output/CampfireTools.dll libs/CampfireTools.dll
```

2. If library is referenced as project then in dockerfile copy its source code and build along with the project.  
!!! The problem is that we need to assure that relative path in local csproj as well as csproj file in docker image are valid.
And since csproj file is just being copied it means that sometimes there is no way to assure this. 
Eg. On host project is in `/home/test/programs/docker/my-project` and lib is in `/home/test/libs/my-lib`
While on host it will be in /app and lib will be copied to /libs/my-lib. The relative path won't match!
```
# --- COPY REFERENCED LIBRARIES ---
ARG CAMPFIRE_TOOLS_DIR_PATH=../../campfire-tools/

WORKDIR /libs/campfire-tools
COPY ${CAMPFIRE_TOOLS_DIR_PATH} .
```

## Web scrapping require chromedriver and chrome browser.
To automate this, add to the dockerfile following lines:
```
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime_prepared

RUN apt-get update

# Install dependencies
RUN apt-get install -y wget gpg curl unzip

# Download latest chromedriver
RUN latest_chrome_version=$(curl -s "https://googlechromelabs.github.io/chrome-for-testing/LATEST_RELEASE_STABLE") \
    && echo "Downloading chromedriver for version: $latest_chrome_version" \
    && wget -N https://edgedl.me.gvt1.com/edgedl/chrome/chrome-for-testing/${latest_chrome_version}/linux64/chromedriver-linux64.zip -O /tmp/chromedriver-linux64.zip

# Chrome driver will be waiting in /chromedriver
RUN unzip -oj /tmp/chromedriver-linux64.zip -d /

# Install latest chrome browser
RUN wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb
RUN apt-get -y install ./google-chrome-stable_current_amd64.deb
```

For the reference see BountyHunterAI project.


