{ pkgs ? import <nixpkgs> {} }:

with pkgs;

stdenv.mkDerivation rec {
  name = "flash-multi";

  src = ./.;

  nativeBuildInputs = [ autoPatchelfHook ];
  buildInputs = [ libusb ];

  installPhase = ''
    runHook preInstall
    mkdir -p $out/{bin,${name}}
    mv * $out/${name}/
    ln -s $out/${name}/flash-multi $out/bin/
    ln -s $out/${name}/multi-bootreloader $out/bin/
    runHook postInstall
  '';
}
