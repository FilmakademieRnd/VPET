/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tools
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2020 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET component Unreal Scene Distribution is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the Unreal Scene Distribution is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the Unreal Scene Distribution Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Unreal Scene Distribution in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/

#pragma once

#include <zmq.hpp>

#include "EngineUtils.h"
//#include "deque"

#include "Engine/Light.h"
#include "Engine/PointLight.h"
#include "Components/PointLightComponent.h"
#include "Engine/SpotLight.h"
#include "Components/SpotLightComponent.h"
#include "Components/LocalLightComponent.h"

#include "Camera/CameraActor.h"
#include "Camera/CameraComponent.h"

#include "Materials/MaterialInterface.h"
#include "Materials/Material.h"
#include "MaterialExpressionIO.h"
#include "Math/Color.h"
#include "Engine/Texture.h"

#include "SceneDistributionState.h"
#include "Kismet/GameplayStatics.h"
#include "Kismet/KismetMathLibrary.h"

// for layer dev
#include "Layers/Layer.h"
// #include "Layers/LayersSubsystem.h" // only >= 4.24

// for casting tests
#include "Materials/MaterialExpressionConstant3Vector.h"
#include "Materials/MaterialExpressionConstant4Vector.h"

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
	AVPETModule();

protected:
	virtual void BeginPlay() override;
	virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;

public:
	virtual void Tick(float DeltaTime) override;

	// Editor Details Settings

	// IP Address of the host server
	UPROPERTY(EditAnywhere, Category = "VPET Settings")
		FString HostIP;

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
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogBasic;
	// Logging level: material
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogMaterial;
	// Logging level: attachment
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogAttachment;
	// Logging level: tag
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogTag;
	// Logging level: folder
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogFolder;
	// Logging level: layer
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogLayer;
	// Logging level: geometry build
	UPROPERTY(EditAnywhere, Category = "VPET Settings|Development|Logging")
		bool LogGeoBuild;


	// ZMQ settings
	zmq::context_t*	context;
	zmq::socket_t* socket_d;
	zmq::socket_t* socket_s;

	// Variable monitored by thread
	bool kEval = true;

	// Host ID
	uint8_t m_id;

	// Message Buffer
	//std::deque<uint8_t*> msgQ;
	//std::vector<uint8_t*> msgQ;
	std::vector<std::vector<uint8_t>> msgQ;

	// List to be populated from host
	TArray<AActor*> actorList;

	// Debug - Control bool 
	bool doItOnce = true;

	// From VPETRegister.cs
	enum ParameterType
	{
		POS, ROT, SCALE, LOCK, HIDDENLOCK, KINEMATIC, // node
		FOV, ASPECT, FOCUSDIST, FOCUSSIZE, APERTURE,   // camera
		COLOR, INTENSITY, EXPOSURE, RANGE, ANGLE, // light
		BONEANIM, // animation bone
		VERTEXANIM, // animation vertex
		PING, RESENDUPDATE,  // sync and ping
		CHARACTERTARGET
	};

private:
	// Distribution state
	VPET::SceneDistributorState m_state;

	void buildWorld();
	bool buildLocation(AActor *prim);
	void buildNode(VPET::NodeGeo *node, AActor* prim);
	void buildNode(VPET::NodeCam *node, AActor* prim);
	void buildNode(VPET::NodeLight *node, AActor* prim, FString className);

	void buildEmptyRotator(FString parentName);

	// World Settings
	AWorldSettings *wrldSet;
	float lgtMult = 1.0;

	//void ParseTransformation(uint8_t* kMsg);
	void ParseTransformation(std::vector<uint8_t> kMsg);

	void AddActorPointer(AActor *pActor) {
		actorList.Add(pActor);
	}

};


// Distribution thread
class ThreadDistDev : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<ThreadDistDev>;
public:
	zmq::socket_t* socket;
	VPET::SceneDistributorState* m_sharedState;
	bool doLog;

	ThreadDistDev(zmq::socket_t* pSocket, VPET::SceneDistributorState* pState, bool pLog) : socket(pSocket), m_sharedState(pState), doLog(pLog) { }

	void DoWork();

	FORCEINLINE TStatId GetStatId() const
	{
		RETURN_QUICK_DECLARE_CYCLE_STAT(FGenericTask, STATGROUP_TaskGraphTasks);
	}

};


// Synchronization thread
class ThreadSyncDev : public FNonAbandonableTask
{
	friend class FAutoDeleteAsyncTask<ThreadSyncDev>;
public:
	zmq::socket_t* socket;
	//std::vector<uint8_t*>* msgQ;
	std::vector<std::vector<uint8_t>>* msgQ;
	bool doLog;

	//ThreadSyncDev(zmq::socket_t* pSocket, std::vector<uint8_t*>* pQueue, bool pLog) : socket(pSocket), msgQ(pQueue), doLog(pLog) { }
	ThreadSyncDev(zmq::socket_t* pSocket, std::vector<std::vector<uint8_t>>* pQueue, bool pLog) : socket(pSocket), msgQ(pQueue), doLog(pLog) { }

	void DoWork();

	FORCEINLINE TStatId GetStatId() const
	{
		RETURN_QUICK_DECLARE_CYCLE_STAT(FGenericTask, STATGROUP_TaskGraphTasks);
	}

};
