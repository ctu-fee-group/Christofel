{ buildDotnetGlobalTool,
  lib,
  dotnet-sdk_8,
  dotnet-runtime_8
}:

buildDotnetGlobalTool {
  pname = "dotnet-ef";
  version = "8.0.6";

  dotnet-runtime = dotnet-runtime_8;
  dotnet-sdk = dotnet-sdk_8;

  nugetSha256 = "sha256-OAerJg3CZRg7xxRB8AKzgPsPY2ubPQ5MjMEmGb2mlQE=";

  meta = {
    license = lib.licenses.mit;
    platforms = lib.platforms.linux;
  };
}

# TODO: get this to build
# { lib,
#   fetchFromGitHub,
#   buildDotnetModule,
#   dotnet-sdk_8,
#   dotnet-runtime_8
# }:

# buildDotnetModule rec {
#   pname = "dotnet-ef";
#   version = "8.0.6";

#   src = fetchFromGitHub {
#     owner = "dotnet";
#     repo = "efcore";
#     rev = "refs/tags/v${version}";
#     hash = "sha256-Hyz/aMaHwAh15ul4VI3LRQOqGQ4ZLG0ilL+EuKxZ3wI=";
#   };

#   dotnet-sdk = dotnet-sdk_8;
#   dotnet-runtime = dotnet-runtime_8;

#   nugetDeps = ./deps.nix;
#   projectFile = "src/dotnet-ef/dotnet-ef.csproj";

#   meta = {
#     homepage = "https://github.com/dotnet/efcore";
#     license = lib.licenses.mit;
#   };
# }
