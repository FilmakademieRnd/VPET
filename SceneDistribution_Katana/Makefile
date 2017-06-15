# Katana
export KATANA_HOME=/opt/katana2.1v4
# GLM
export GLM=/Dreamspace/ext/glm
# ZMQ devel libs
export ZMQ_HOME=${KATANA_HOME}/bin/ZeroMQ

all:
	$(MAKE) -C src/Render
	$(MAKE) -C src/ScenegraphLocationDelegate
	$(MAKE) -C src/RendererInfo
	@echo "Build done."

clean:
	$(MAKE) -C src/Render clean
	$(MAKE) -C src/ScenegraphLocationDelegate clean
	$(MAKE) -C src/RendererInfo clean

