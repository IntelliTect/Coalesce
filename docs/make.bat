@ECHO OFF


pushd %~dp0

REM Command file for Sphinx documentation

if "%SPHINXBUILD%" == "" (
	set SPHINXBUILD=python -msphinx
)
set SOURCEDIR=.
set BUILDDIR=_build/html
set SPHINXPROJ=Coalesce


%SPHINXBUILD% >NUL 2>NUL
if errorlevel 9009 (
	echo "The Sphinx module was not found. Make sure you have Python 3.4 installed and on your path,"
	echo "and Sphinx installed as well (pip install sphinx)."
	exit /b 1
)
@ECHO ON
%SPHINXBUILD% -b html %SOURCEDIR% %BUILDDIR% %SPHINXOPTS%
@ECHO OFF
goto end


:end
popd
