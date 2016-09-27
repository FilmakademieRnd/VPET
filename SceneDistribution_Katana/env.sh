####################
# REQUIRED libs
####################
# path to where katana or katanaPlayer was installed
export KATANA_HOME=/Foundry/Katana/Katana2.0v1b5-linux-x86-release-64
export KATANA_PLAYER_HOME=/Foundry/KatanaPlayer/KatanaPlayer1.0FMX-linux-x86-release-64

export KATANA_RESOURCES=/Dreamspace/OpenRL1.0v1a2/Resources

# path to boost devel libs
# tested with v. 1.46.0
export BOOST_HOME=usr/lib64
export BOOST_INCLUDE=$BOOST_HOME/../include

export PATH=$PATH:$KATANA_HOME/bin

export GLM=/Dreamspace/OpenRL1.0v1a2/OpenRLPlugin/src/glm


#export SOIL=/Dreamspace/OpenRL1.0v1a2/OpenRLPlugin/src/soil
#export GLEW=/Dreamspace/OpenRL1.0v1a2/OpenRLPlugin/src/glew-1.13.0



# path to zeromq devel libs
# NOTE: required by the api only if building display drivers
# tested with v. 3.2.2.rc2
export ZMQ_HOME=/Foundry/Katana/Katana2.0v1b5-linux-x86-release-64/bin/ZeroMQ

####################
# OPTIONAL libs
####################
# path to openexr/ilmbase devel libs
# used in the openrl plugin to manipulate matrices
# tested with v. 1.6.1
#export OPENEXR_HOME=/workspace/burgess/Thirdparty/OpenEXR/1.6.1/bin/linux-64-x86-release-410-gcc

