@call PubConfig.cmd
%~dp0res\S3Sync\S3Sync.exe -AWSAccessKeyId %AWSAccessKeyId% -AWSSecretAccessKey %AWSSecretAccessKey% -SyncDirection upload -LocalFolderPath "bin\Debug\app.publish" -BucketName download.livereload.com -S3FolderKeyName windows-stage/ -UploadHeaders x-amz-acl:public-read -DeleteS3ItemsWhereNotInLocalList false -UseSSL false -TransferThreads 5 -MultipartThreads 1
