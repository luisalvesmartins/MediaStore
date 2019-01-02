# MediaStoreSite
Browse and admin your digital media in Azure

Two sets of files, browse media and manage the site.
The site is all made of static webpages and the data is provided by the api deployed in Azure functions.
Authentication is managed with facebook login.

# Browse

main.html provides a list of all the folders the user can see.
When the user clicks on a folder he is redirected to the detail.html page.

# Manage

admin.html enables all the tagging, moving, renaming and media deletion.
permissions.html provide the solution to manage user permissions.
