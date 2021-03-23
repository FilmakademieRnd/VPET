#git log --format="%an <%ae>" VPET_Unity2/Assets/VPET/Modules/SceneManagerModules/SceneLoaderModule/Scripts/SceneLoaderModule.cs | sort -u

import subprocess
import datetime

gitParams = ['A', 'M']

def run(command):
	return subprocess.run(command, shell=True, check=True, stdout=subprocess.PIPE, universal_newlines=True).stdout

date = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
author = run('git config user.name').strip()
authors = ' '
email = run('git config user.email').strip()

process = run('git status -s')
output = process
gitLines = output.split('\n')
for line in gitLines:
	if len(line) > 0:
		lineParams = line.rsplit(' ', 1)
		filePath = lineParams[1]
		if any(p in lineParams[0] for p in gitParams):
			if 'M' in lineParams[0]:
				authors = run('git log --format="%an <%ae>" ' + filePath + ' | sort -u');
				print (authors)

			
		
		
		
		