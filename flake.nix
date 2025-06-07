{
  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  inputs.flake-parts.url = "github:hercules-ci/flake-parts";
  inputs.process-compose-flake.url = "github:Platonic-Systems/process-compose-flake";
  inputs.services-flake.url = "github:juspay/services-flake";

  outputs = inputs:
    inputs.flake-parts.lib.mkFlake { inherit inputs; } {
      imports = [
        inputs.process-compose-flake.flakeModule
      ];

      systems = [
        "x86_64-linux"
      ];

      perSystem = { self', pkgs, lib, ... }: let
        dotnet-ef = pkgs.callPackage ./nix/dotnet-ef.nix {};
      in {
        # 3. Create the process-compose configuration, importing services-flake
        process-compose."christofel-services" = {
          imports = [
            inputs.services-flake.processComposeModules.default
          ];

          services.mysql."christofel-db" = {
            enable = true;
            package = pkgs.mysql80;
            initialDatabases = [
              { name = "christofel"; }
            ];

            ensureUsers = [
              {
                name = "christofel";
                password = "christofel";
                ensurePermissions = {
                  "christofel.*" = "ALL PRIVILEGES";
                };
              }
            ];
          };

          settings.processes.christofel-migrator = {
            depends_on."christofel-db-configure".condition = "process_completed_successfully";
            command = ''
              echo $(date): Migrating database...
              ${lib.getExe self'.packages.christofel-migrate}
              echo $(date): Done.
            '';
          };
        };

        packages.christofel-migrate = let
          configFile = pkgs.writers.writeJSON "config.json" {
            ConnectionStrings = {
              "ChristofelBase" = "Server=localhost;Database=christofel;Uid=christofel;Pwd=christofel";
            };
          };
        in pkgs.writeShellApplication {
          name = "christofel-migrate";

          runtimeInputs = [
            pkgs.dotnet-sdk_8
          ];

          text = ''
            set -euox pipefail
            export CHRISTOFEL_CONFIG_PATH=${configFile}
            dotnet tool restore
            dotnet ef database --startup-project=./src/Tools/Christofel.Design/ update --context ChristofelBaseContext -p ./src/Core/Christofel.Common
            dotnet ef database --startup-project=./src/Tools/Christofel.Design/ update --context ManagementContext -p ./src/Plugins/Christofel.Management
            dotnet ef database --startup-project=./src/Tools/Christofel.Design/ update --context ReactHandlerContext -p ./src/Plugins/Christofel.ReactHandler
            dotnet ef database --startup-project=./src/Tools/Christofel.Design/ update --context ApiCacheContext -p ./src/Plugins/Christofel.Api
            dotnet ef database --startup-project=./src/Tools/Christofel.Design/ update --context CoursesContext -p ./src/Libs/Christofel.CoursesLib
          '';
        };

        devShells.default = pkgs.mkShell {
          name = "christofel-dev";
          packages = [
            # Dotnet deps
            pkgs.dotnet-sdk_8
            pkgs.dotnet-runtime_8

            # Services
            pkgs.mysql80

            # Development, debugging
            pkgs.csharp-ls
            pkgs.netcoredbg

            pkgs.dotnet-outdated
            dotnet-ef
          ];
        };
      };
    };
}
