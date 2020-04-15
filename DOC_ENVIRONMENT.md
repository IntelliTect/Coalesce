Setting Up VS Code With reStructuredText Tools
==============================================

* [Software](#software)
* [VS Code](#vs-code)
* [Configuration](#configuration)

## Software

* [Windows](#windows)
* [Linux](#linux)

### Windows
* Download the latest stable point release of Python 2 for your windows architecture from [here](https://www.python.org/downloads/windows/).
* Download the "executable installer" flavor of the binary (.msi file)
* Assuming you took the default installation locaion, use PowerShell to add Python to your %PATH% environment variable:
```PowerShell
PS> [Environment]::SetEnvironmentVariable("Path", "$env:Path;C:\Python27\;C:\Python27\Scripts\", "User")
```
* Restart your PowerShell session
* Download the pip bootstrap script from [here](https://bootstrap.pypa.io/get-pip.py).
* Install pip via Powershell:
```PowerShell
PS> python .\get-pip.py
```
* Install Sphinx via Powershell:
```PowerShell
PS> pip install sphinx sphinx-tabs
```
* Install the reStructuredText linter using pip:
```PowerShell
PS> pip install restructuredtext_lint
```

### Linux
* For Debian/Ubuntu: (other distros YMMV)
* Install python-sphinx package
```
$ apt-get install python-sphinx
```
* Install python-restructured-lint package
```
$ apt-get install python-restructuredtext-lint 
```

## VS Code
* Download and install Visual Studio Code [here](https://code.visualstudio.com/Download).
* Launch VS Code and install the reStructuredText extension from [LeXtudio](https://marketplace.visualstudio.com/items?itemName=lextudio.restructuredtext).

## Command-line 
* The docs may also be built from the command-line. Inside the ``docs`` folder, run either ``make.bat`` or ``make`` using the included ``makefile``, depending on your platform. Output will be in ``_build/html`` with ``index.html`` being the home page.

## Configuration
* Open the ``docs`` folder of your cloned Coalesce repo in VS Code
* Open the settings file (``Ctrl + ,``)
* Set the following value to the path to the reStructuredText linter:
```
"restructuredtext.linter.executablePath": "C:\\Python27\\Scripts\\restructuredtext-lint.exe"
```
* Open a .rst in VS Code
* Click the Preview icon (``Ctrl + K V``)
* You should see a preview of the document
