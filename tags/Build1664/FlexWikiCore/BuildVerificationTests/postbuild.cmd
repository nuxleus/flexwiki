@rem arguments are projectdir targetdir targetpath

@rem copy app.config to target.dll.config so NUnit can find it
copy %1app.config %3.config