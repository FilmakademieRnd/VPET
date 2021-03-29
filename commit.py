import subprocess
import datetime
import os

gitFlags = ['A', 'M']
fileExtensions = ['cpp', 'h', 'hpp', 'cs']
headerFile = open('LICENSE.TXT', 'r').read()


def run(command):
	return subprocess.run(command, shell=True, check=True, stdout=subprocess.PIPE, universal_newlines=True).stdout
	
def createHeader(filepath):
	filename = filepath.split('/')[-1]
	return headerFile + '\n' \
	+ '//! @file "' + filename + '"' + '\n' \
	+ '//! @last author ' + author + '\n' \
	+ '//! @past authors ' + authors + '\n' \
	+ '//! @version ' + version + '\n' \
	+ '//! @date ' + date + '\n'
	
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
	
def addHeader(filepath):
	with open(filepath, 'r+') as file:
		file.write(createHeader(filepath) + stripHeader(filepath))
		
date = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
author = run('git config user.name').strip()
authors = ' '
email = run('git config user.email').strip()
version = '0'

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
					authorString = (run('git log --format="%an <%ae>" ' + filePath + ' || sort -u'));
					authors = ' '.join(authorString.split('\n'))
				print (filePath)
				addHeader(filePath)

			
headerFile.close()
		
		
		