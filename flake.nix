{
  description = "Christofel development flake";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-23.05";
  };

  outputs = { self, nixpkgs }: let
    pkgs = nixpkgs.legacyPackages.x86_64-linux;
  in {
    packages.x86_64-linux.dotnet = pkgs.dotnet-sdk_7;
    packages.x86_64-linux.mysql = pkgs.mysql80;
    packages.x86_64-linux.dotnet-runtime = pkgs.dotnet-runtime_7;
    packages.x86_64-linux.default = self.packages.x86_64-linux.dotnet;

    devShells.x86_64-linux.default = pkgs.mkShell {
      name = "christofel";

      nativeBuildInputs = with pkgs; [
        self.packages.x86_64-linux.dotnet
        self.packages.x86_64-linux.mysql
      ];

      buildInputs = with pkgs; [
        self.packages.x86_64-linux.dotnet-runtime
      ];

      shellHook = ''
        alias db="mysql -uroot -proot --host 127.0.0.1 -D christofel"
        dotnet tool restore
      '';
    };
  };
}
