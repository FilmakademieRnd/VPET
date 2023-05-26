// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#define DEG2RAD (3.14159265/180.0)

#include <zmq.hpp>
#include "SceneDistributorState.h"
#include "EngineUtils.h" // for TActorIterator
#include "Kismet/KismetMathLibrary.h" // for UKismetMathLibrary::MakeRelativeTransform

// local classes
#include "SceneSenderThread.h"
#include "UpdateReceiverThread.h"
#include "UpdateSenderThread.h"

// for casting tests
#include "Materials/MaterialExpressionConstant3Vector.h"
#include "Materials/MaterialExpressionConstant4Vector.h"

// for lights
#include "Engine/Light.h"
#include "Engine/PointLight.h"
#include "Components/PointLightComponent.h"
#include "Engine/SpotLight.h"
#include "Components/SpotLightComponent.h"
#include "Engine/RectLight.h"
#include "Components/RectLightComponent.h"
#include "Components/LocalLightComponent.h"

// for level editor
#include "LevelEditor.h"

// for camera
#include "Camera/CameraActor.h"
#include "Camera/CameraComponent.h"

#include "MaterialExpressionIO.h" // for material
#include "Materials/Material.h" // for material

// Sub component
#include "SceneObject.h"
#include "SceneObjectLight.h"
#include "SceneObjectAreaLight.h"
#include "SceneObjectDirectionalLight.h"
#include "SceneObjectPointLight.h"
#include "SceneObjectSpotLight.h"
#include "SceneObjectCamera.h"
// Stats
#include "Kismet/GameplayStatics.h"


#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "VPETModule.generated.h"



// Development output log macro
#define DOL(logType, logVerbosity, logString, ...) if(logType) UE_LOG(LogTemp, logVerbosity, TEXT(logString), __VA_ARGS__);

// Development on-screen debug message
#define OSD(debugColor, debugString, ...) if(GEngine && VerboseDisplay) GEngine->AddOnScreenDebugMessage(-1, 5.0f, debugColor, FString::Printf(TEXT(debugString), __VA_ARGS__));



UCLASS()
class VPET_API AVPETModule : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	AVPETModule();

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

public:
	virtual void Tick(float DeltaTime) override;

	// IP Address of the host server
	UPROPERTY(EditAnywhere, Category = "VPET Settings")
		FString HostIP;

	// Set to use textures (requires prior setup)
	UPROPERTY(EditAnywhere, Category = "VPET Settings")
		bool UseTexture;

	// Multiplying factor for light intensity
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Light settings")
		float BrightnessMultiplier;

	// Multiplying factor for light range (only relevant for point and spot lights)
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Light settings")
		float RangeMultiplier;

	// Send the attached objects under lights
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Light settings")
		bool SendLightChild;

	// Only send to VPET the actors that are tagged with LodLow or LodMix; if false, send all
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development")
		bool UseSendTag;

	// Only treat as editable the actors with tag "editable"; if false, every movable object is editable
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development")
		bool UseEditableTag;

	// Display screen debug messages
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development")
		bool VerboseDisplay;

	// Logging level: basic
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogBasic;
	// Logging level: material
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogMaterial;
	// Logging level: attachment
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogAttachment;
	// Logging level: tag
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogTag;
	// Logging level: folder
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogFolder;
	// Logging level: layer
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogLayer;
	// Logging level: geometry build
	UPROPERTY(EditAnywhere, Category = "VPET2 Settings|Development|Logging")
		bool LogGeoBuild;

	// Development print latch
	bool doItOnce = true;

	// ZMQ settings
	zmq::context_t* context;
	zmq::socket_t* socket_d;
	zmq::socket_t* socket_r;
	zmq::socket_t* socket_s;

	// Message buffer
	std::vector<std::vector<uint8_t>> msgQ;

	// Message buffer for sending
	std::vector<std::vector<uint8_t>> msgQs;
	// Binary buffer for message construction
	std::vector<char*> msgData;
	std::vector<int> msgLen;

	// Host ID
	uint8_t m_id;

	// List to be populated from host
	TArray<AActor*> actorList;

	// List of selected objects in the editor
	TArray<UObject*> selectedList;

	// List of scene objects - for easier parameter access
	TArray<USceneObject*> VPET_SceneObjectList;

	//List of modified parameters
	TArray<AbstractParameter*> VPET_ModifiedParameterList;
	//Size of modified parameters
	int VPET_modifiedParametersDataSize;
	//Create parameter MSG function
	void CreateParameterMessage();

	
	//UFUNCTION()
	void HasChangedIsCalled( AbstractParameter* param);

	// Material name list - for texture identification
	TArray<FString> matNameList;

	// Object type - for attaching the right component types to it
	enum class ObjectType
	{
		NODE, GEO, LIGHT, CAMERA, DIRECTIONALLIGHT, SPOTLIGHT, POINTLIGHT, AREALIGHT
	};

	// Scene Object
	enum class ParameterType
	{
		POSITION, ROTATION, SCALE
	};

	// Scene Object Light
	enum class ParameterTypeLight
	{
		POSITION, ROTATION, SCALE,
		COLOR, INTENSITY
	};

	// Scene Object Camera
	enum class ParameterTypeCamera
	{
		POSITION, ROTATION, SCALE,
		FOV, ASPECTRATIO, NEARCLIPPLANE, FARCLIPPLANE, FOCALDISTANCE, APERTURE
	};


private:
	// Distributor state
	VPET::SceneDistributorState m_state;

	void buildWorld();
	bool buildLocation(AActor* prim);
	void buildNode(VPET::NodeGeo* node, AActor* prim);
	void buildNode(VPET::NodeCam* node, AActor* prim);
	void buildNode(VPET::NodeLight* node, AActor* prim, FString className);

	void buildEmptyRotator(FString parentName);

	// World Settings
	AWorldSettings* wrldSet;
	float lgtMult = 1.0;

	void ParseParameterUpdate(std::vector<uint8_t> kMsg);
	

	void AddActorPointer(AActor* pActor) {
		actorList.Add(pActor);
	}

	// Editor selection handler
	void HandleOnActorSelectionChanged(const TArray<UObject*>& NewSelection, bool bForceRefresh);

public:

	void EncodeLockMessage(int16_t objID, bool lockState);

	void DecodeLockMessage(int16_t* objID, bool* lockState);
};
