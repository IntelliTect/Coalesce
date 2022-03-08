Setting Up VS Code With reStructuredText Tools
==============================================

* [Software](#software)
* [VS Code](#vs-code)
* [Configuration](#configuration)

## Software

* [Windows](#windows)
* [Linux](#linux)

### Windows
* Download the latest stable point release of Python 3 from [here](https://www.python.org/downloads/). Be sure to select the following options in the installer:
  * install `pip`
  * Add Python to environment variables
* Install required Python packages:
```PowerShell
> pip install sphinx sphinx-tabs rstcheck esbonio
```
* Restart VS Code if you did the VS code setup steps first.

## VS Code
* Download and install Visual Studio Code [here](https://code.visualstudio.com/Download).
* Launch VS Code and install the reStructuredText extension from [LeXtudio](https://marketplace.visualstudio.com/items?itemName=lextudio.restructuredtext).

## Command-line 
* The docs may also be built from the command-line. Inside the ``docs`` folder, run either ``make.bat`` or ``make`` using the included ``makefile``, depending on your platform. Output will be in ``_build/html`` with ``index.html`` being the home page.

## Configuration
* Open the ``docs`` folder of your cloned Coalesce repo in VS Code
* Open the settings file (``Ctrl + ,``)
* Open a .rst in VS Code
* Click the Preview icon (``Ctrl + K V``)
* You should see a preview of the document
