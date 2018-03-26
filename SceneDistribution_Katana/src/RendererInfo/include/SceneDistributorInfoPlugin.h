/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

The VPET components Scene Distribution and Synchronization Server are intended
for research and development purposes only. Commercial use of any kind is not 
permitted.

There is no support by Filmakademie. Since the Scene Distribution and 
Synchronization Server are available for free, Filmakademie shall only be 
liable for intent and gross negligence; warranty is limited to malice. Scene 
Distribution and Synchronization Server may under no circumstances be used for 
racist, sexual or any illegal purposes. In all non-commercial productions, 
scientific publications, prototypical non-commercial software tools, etc. 
using the Scene Distribution and/or Synchronization Server Filmakademie has 
to be named as follows: “VPET-Virtual Production Editing Tool by Filmakademie 
Baden-Württemberg, Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Scene Distribution and/or 
Synchronization Server in a commercial surrounding or for commercial purposes, 
software based on these components or any part thereof, the company/individual 
will have to contact Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/
#ifndef COMPANYNAMEKATANA_SCENEDISTRIBUTORINFOPLUGIN_H
#define COMPANYNAMEKATANA_SCENEDISTRIBUTORINFOPLUGIN_H

#include <FnRendererInfo/plugin/RendererInfoBase.h>


namespace Dreamspace
{
namespace Katana
{
    /**
     * \ingroup SceneDistributorPlugin
     */

    /**
     * @brief SceneDistributor RendererInfo Plugin
     */
    class SceneDistributorInfoPlugin : public FnKat::RendererInfo::RendererInfoBase
    {
    public:

        SceneDistributorInfoPlugin();
        virtual ~SceneDistributorInfoPlugin();

        /**
        * fillRenderMethods
        */
        void fillRenderMethods(std::vector<FnKat::RendererInfo::RenderMethod*>& renderMethods) const;

        /**
        * fillRendererObjectNames
        */
        void fillRendererObjectNames(std::vector<std::string>& rendererObjectNames,
                                     const std::string& type,
                                     const std::vector<std::string>& typeTags) const;

        /**
        * fillRendererObjectTypes
        */
       void fillRendererObjectTypes(std::vector<std::string>& renderObjectTypes,
                                    const std::string& type) const;

        /**
        * configureBatchRenderMethod
        */
        void configureBatchRenderMethod(FnKat::RendererInfo::DiskRenderMethod& batchRenderMethod) const;

        /**
        * Registered renderer name that corresponds to this renderer info
        *
        * @param result The renderer plug-in that corresponds to this renderer info plug-in
        */
        std::string getRegisteredRendererName() const;

        /**
        * Registered version of the renderer this renderer info is used with.
        *
        * @param result The renderer version
        * @see getRegisteredRendererName
        */
        std::string getRegisteredRendererVersion() const;

        /**
        * getRendererObjectDefaultType
        */
        std::string getRendererObjectDefaultType(const std::string& type) const;

        /**
        * Declares if a renderer output requires a pre-declared temp file
        * (accessible in scene graph with implicit resolvers).
        *
        * @param outputType A render output type
        * @return true if a local file is needed, false otherwise
        */
        bool isPresetLocalFileNeeded(const std::string& outputType) const;

        /**
        * Katana will call this function to determine if the renderer supports
        * specific nodes. Currently ShadingNode and OutputChannelDefine will be
        * queried. True should be returned if the renderer supports this node type.
        *
        * @return true if the node type is supported, false otherwise
        */
        bool isNodeTypeSupported(const std::string& nodeType) const;

        /**
        * Declares if polymesh faces are split into sub-meshes where each
        * mesh represents a single face set as required by some renderers.
        *
        * @return true if splitting is enabled, false otherwise
        */
        bool isPolymeshFacesetSplittingEnabled() const;

        /**
        * Shader Inputs / Outputs
        *
        */
        void fillShaderInputNames(std::vector<std::string>& shaderInputNames,
                                  const std::string& shaderName) const;

        /**
        * fillShaderInputTags
        */
        void fillShaderInputTags(std::vector<std::string>& shaderInputTags,
                                 const std::string& shaderName,
                                 const std::string& inputName) const;

        /**
        * fillShaderOutputNames
        */
        void fillShaderOutputNames(std::vector<std::string>& shaderOutputNames,
                                   const std::string& shaderName) const;

        /**
        * fillShaderOutputTags
        */
        void fillShaderOutputTags(std::vector<std::string>& shaderOutputTags,
                                  const std::string& shaderName,
                                  const std::string& outputName) const;

        /**
        * fillRendererShaderTypeTags
        */
        void fillRendererShaderTypeTags(std::vector<std::string>& shaderTypeTags,
                                        const std::string& shaderType) const;

        /**
        * getRendererCoshaderType
        */
        std::string getRendererCoshaderType() const;

        /**
        * buildRendererObjectInfo
        */
        bool buildRendererObjectInfo(FnKat::GroupBuilder& rendererObjectInfo,
                                     const std::string& name,
                                     const std::string& type,
                                     const FnKat::GroupAttribute inputAttr) const;

        void fillLiveRenderTerminalOps(OpDefinitionQueue& terminalOps,
                                       const FnAttribute::GroupAttribute& stateArgs) const;

        /**
         * Initialise the RendererInfo object. May involve loading shaders.  Called after
         * paths are set, but before Renderer Info is interrogated.
         */
        void initialiseCaches();

        /**
        * flushCaches
        */
        void flushCaches();

        static FnKat::RendererInfo::RendererInfoBase* create()
        {
            return new SceneDistributorInfoPlugin();
        }

        static void flush()
        {

        }
    };

    /**
     * @}
     */
}
}

#endif
