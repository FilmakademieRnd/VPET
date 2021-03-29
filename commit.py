import subprocess
import datetime
import os

# ' ' = unmodified
# M = modified
# A = added
# D = deleted
# R = renamed
# C = copied
# U = updated but unmerged
# use only files with this git flags
gitFlags = ['A', 'M']

# use only files with this extension
fileExtensions = ['cpp', 'h', 'hpp', 'cs']

# filepath to license text
headerFile = open('LICENSE.TXT', 'r').read()

# function for executing shell commands and capure theyr output
def run(command):
	return subprocess.run(command, shell=True, check=True, stdout=subprocess.PIPE, universal_newlines=True).stdout

# function creating new header: license + infos	
def createHeader(filepath):
	filename = filepath.split('/')[-1]
	return headerFile + '\n' \
	+ '//! @file "' + filename + '"' + '\n' \
	+ '//! @last author ' + author + '\n' \
	+ '//! @past authors ' + authors + '\n' \
	+ '//! @version ' + version + '\n' \
	+ '//! @date ' + date + '\n'

# function for removing old header from file	
def stripHeader(filepath):
	with open(filepath, 'r') as hfile:
		lines = hfile.readlines()
		index = -1
		for i, s in enumerate(lines):
			if '//! @date' in s:
				index = i
		lines = lines[index+1:]
		print(index)
		return ''.join(lines)

# function writing new header to file
def addHeader(filepath):
	with open(filepath, 'r+') as file:
		file.write(createHeader(filepath) + stripHeader(filepath))

# the current date and time		
date = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
# gather local author name
author = run('git config user.name').strip()
authors = ' '
# gather local author email
email = run('git config user.email').strip()
# gather the file version info
version = run('git rev-list --count HEAD').strip()

process = run('git status -s')
output = process
gitLines = output.split('\n')
for line in gitLines:
	if len(line) > 0:
		gitParams = line.rsplit(' ', 1)
		filePath = gitParams[1]
		if any(p in gitParams[0] for p in gitFlags):
			fileExtension = os.path.splitext(filePath)[1][1:].strip().lower()
			if fileExtension in fileExtensions:
				if 'M' in gitParams[0]:
					# gather all authors if file has been modified
					authorString = (run('git log --format="%an <%ae>" ' + filePath + ' || sort -u'));
					authors = ' '.join(authorString.split('\n'))
				print (filePath)
				addHeader(filePath)

			
headerFile.close()
		
		
		