# Zipper
A function app for streaming blobs into memory, zipping them in-stream and then uploading the stream back to Azure Blob storage.


# What is this?
Zipper is an Azure Function. It's main focus is to stream Block blobs from Azure Storage, zip them and then upload a single zipped blob back to the storage account.


# Future goals
I would like to be able to query a database to pull blob references and specifically download those blobs, I would also like to be able to filter via metadata.
