#fancygit
fancy-git is a very good tool to beautify your terminal for git:
https://github.com/diogocavilha/fancy-git/tree/master

However, when you install it under WSL, like Fedora/Ubuntu,
after you have installed fancy-git, you found that the symbols can't show correctly in the terminal.

To solve this issue, you need to install powerline fonts under Windows,
and set the correct fonts for Windows Terminal.

The fonts can be found here:
https://github.com/diogocavilha/fancy-git/tree/master/fonts

You can just download these fonts under Windows,
and double click the fonts to install them.

As personal experience, the font works for me is "SourceCodePro+Powerline+Awesome+Regular.ttf".
The name is "'SourceCodePro+Powerline+Awesome Regular".
You can also set this font as the first font in Visual Studio Code font family,
so that terminal under VS Code can show the symbols correctly.

Important note:
You are logging to Linux use Windows Terminal or VS Code Terminal,
but it is Windows to render these symbols, not Linux.
So you have to install these fonts for Windows, not Linux.


# oh-my-posh
https://ohmyposh.dev/
oh-my-posh can be used to customize PowerShell, git bash etc under Windows.

https://learn.microsoft.com/en-us/windows/terminal/tutorials/custom-prompt-setup
## Install oh-my-posh under Windows
```powershell
winget install JanDeDobbeleer.OhMyPosh
```

## Create powershell profile
```powershell
new-item -type file -path $profile -force
```

The content of the profile is:
```powershell
oh-my-posh init pwsh --config "$env:POSH_THEMES_PATH\paradox.omp.json" | Invoke-Expression
```

This is to use paradox theme.
You can use `Get-PoshThemes` under powershell to get list of themes: https://ohmyposh.dev/docs/themes
Or you can go to https://github.com/JanDeDobbeleer/oh-my-posh/tree/main/themes to browse themes.
The theme name is the json file name.
Environment variable `POSH_THEMES_PATH` points to where themes are installed.

Top themes:
* kushal
* cobalt2
* craver
* capr4n

## Setup Git bash
With git bash under Windows, you can create a `~/.bashrc` and put the following content:
```bash
eval "$(oh-my-posh init bash --config $POSH_THEMES_PATH/paradox.omp.json)"
```
This will setup *paradox* theme for Git bash.

`oh-my-posh --help` to check available commands.

## Install new fonts
You need to install nerd fonts to show some icons, glyphs.
https://github.com/ryanoasis/nerd-fonts/tree/master

You can also use `oh-my-posh font install --user` to install new fonts. Issue above command and select fonts you want to install.
After installation, you may need to restart Windows Terminal to show new fonts.
For each Windows Terminal profile, eg. git bash, you can select the nerd fonts in "Appearance section", or add font manually in Windows Terminal settings json file:
```json
"commandline": "%PROGRAMFILES%/Git/usr/bin/bash.exe -i -l",
"font": 
{
    "face": "CaskaydiaCove Nerd Font"
},
```

