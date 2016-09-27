/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/v-p-e-t

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
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
