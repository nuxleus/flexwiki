wget http://sourceforge.net/export/sf_tracker_export.php?group_id=113273^&atid=665396 -O bugs.xml
copy /y bugs.xml input.xml
tracker-tidy summary input.xml > intermediate.xml
copy /y intermediate.xml input.xml
tracker-tidy detail input.xml > intermediate.xml
copy /y intermediate.xml input.xml
tracker-tidy old_value input.xml > intermediate.xml
copy /y intermediate.xml input.xml
tracker-tidy text input.xml > intermediate.xml
copy /y intermediate.xml bugs-tidied.xml

wget http://sourceforge.net/export/sf_tracker_export.php?group_id=113273^&atid=665399 -O rfe.xml
copy /y rfe.xml input.xml
tracker-tidy summary input.xml > intermediate.xml
copy /y intermediate.xml input.xml
tracker-tidy detail input.xml > intermediate.xml
copy /y intermediate.xml input.xml
tracker-tidy old_value input.xml > intermediate.xml
copy /y intermediate.xml input.xml
tracker-tidy text input.xml > intermediate.xml
copy /y intermediate.xml rfe-tidied.xml

del bugs.xml
del rfe.xml
del input.xml
del intermediate.xml
