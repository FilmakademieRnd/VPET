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

#include "VPETModule.h"

using namespace VPET;

AVPETModule::AVPETModule()
{
	PrimaryActorTick.bCanEverTick = true;

	// Default values
	HostIP = FString("127.0.0.1");
	UseSendTag = true;
	UseEditableTag = true;
	BrightnessMultiplier = 1.0;
	RangeMultiplier = 1.0;
	SendLightChild = false;
	VerboseDisplay = false;
	LogBasic = true;
	LogMaterial = false;
	LogAttachment = false;
	LogTag = false;
	LogFolder = false;
	LogLayer = false;
	LogGeoBuild = false;


}

void AVPETModule::BeginPlay()
{
	Super::BeginPlay();

	DOL(LogBasic, Warning, "[VPET BeginPlay] Game began.");


	// Test ZMQ
	int major, minor, patch;
	zmq::version(&major, &minor, &patch);
	DOL(LogBasic, Log, "[VPET BeginPlay] ZeroMQ version: v%d.%d.%d", major, minor, patch);


	// Grab world
	UWorld* MyWorld = GetWorld();

	// And level if needed
	ULevel* CurrLevel = MyWorld->GetCurrentLevel();

	// Find world settings
	//wrldSet = FindObject<AWorldSettings>(CurrLevel, *FString("WorldSettings"));
	// Temporary use MonoCullingDistance as custom parameter
	//if (wrldSet)
	//{
	//	lgtMult = wrldSet->MonoCullingDistance;
	//	DOL(LogBasic, Log, "Found world settings, MonoCullingDistance: %f", lgtMult);
	//}

	// Print variable
	DOL(LogBasic, Log, "[VPET BeginPlay] World name: %s", *MyWorld->GetName());

	// Populating the distribution state

	// Header values
	m_state.vpetHeader.lightIntensityFactor = 1.0;
	m_state.vpetHeader.textureBinaryType = 0;

	// set init to all
	m_state.lodMode = ALL;

	// set tagging mode (from Katana)
	m_state.lodMode = TAG;
	m_state.lodTag = "lo";


	DOL(LogBasic, Log, "[VPET BeginPlay] Building scene...");

	// World tweak - creating a global root (perhaps better approach: ask user to attach everything that needs to be transmitted to a root actor)
	buildWorld();

	// Root nodes counter
	int rootChildrenCount = 0;

	// Go over actors
	for (TActorIterator<AActor> aIt(MyWorld); aIt; ++aIt)
	{
		AActor* lActor = *aIt;
		//DOL(LogBasic, Log, "[DIST Scene] Found AActor named: %s", *lActor->GetName());
		//DOL(LogBasic, Log, "[DIST Scene] Found AActor labeled: %s", *lActor->GetActorLabel());

		// If has no parent, try to "build" node
		if (!lActor->GetAttachParentActor())
			if (buildLocation(lActor))
				rootChildrenCount++;


		// Development // check hierarchy nesting/attachment

		// Test GetAttachedActors
		TArray<AActor*> attActors;
		lActor->GetAttachedActors(attActors);
		for (size_t i = 0; i < attActors.Num(); i++)
		{
			DOL(LogAttachment, Log, "[VPET BeginPlay] AActor %s has attachment: %s", *lActor->GetActorLabel(), *attActors[i]->GetActorLabel());
		}
		// Test GetAttachParentActor
		AActor* aParActor = lActor->GetAttachParentActor();
		if (aParActor)
		{
			DOL(LogAttachment, Log, "[VPET BeginPlay] AActor %s has attach parent: %s", *lActor->GetActorLabel(), *aParActor->GetActorLabel());
		}


		// Check folder  -> folder paths are only available in development builds.
		auto fPath = lActor->GetFolderPath();
		DOL(LogFolder, Log, "[VPET BeginPlay] AActor %s is under folder %s", *lActor->GetActorLabel(), *fPath.ToString());


		// Check layer
		TArray<FName> aLayers = lActor->Layers;
		DOL(LogLayer, Log, "[VPET BeginPlay] AActor %s is under layers:", *lActor->GetActorLabel());
		for (size_t i = 0; i < aLayers.Num(); i++)
			DOL(LogLayer, Log, "Layer %d: %d", i, *aLayers[i].ToString());

	}

	// Go over layers - this return ALL instances - both editor and PIE
	//for (TObjectIterator<ULayer> uIt; uIt; ++uIt)
	//{
	//	ULayer* lLayer = *uIt;
	//	DOL(LogLayer, Log, "Found ULayer. Name %s, fname %s, display name %s ", *lLayer->GetName(), *lLayer->GetFName().ToString(), *lLayer->LayerName.ToString());
	//	DOL(LogLayer, Log, "layer ID: %d ", lLayer->GetUniqueID());

	//	// grab stats
	//	TArray <FLayerActorStats> lStats = lLayer->ActorStats;
	//	//DOL(LogBasic, Log, "Num of stats: %d ", lStats.Num());
	//	for (size_t i = 0; i < lStats.Num(); i++)
	//	{
	//		int lTot = lStats[i].Total;
	//		UClass* lType = lStats[i].Type;
	//		DOL(LogLayer, Log, "Stat %d, total %d", i, lTot);
	//		DOL(LogLayer, Log, "type: %s", *lType->GetName());
	//	}
	//}


	// Edit root child count
	m_state.nodeList.at(0)->childCount = rootChildrenCount;
	DOL(LogBasic, Log, "[VPET BeginPlay] Root children count: %i", m_state.nodeList.at(0)->childCount);

	// Print stats
	DOL(LogBasic, Log, "[VPET BeginPlay] Texture Count: %i", m_state.texPackList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Object(Mesh) Count: %i", m_state.objPackList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Node Count: %i", m_state.nodeList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Objects: %i", m_state.numObjectNodes);
	DOL(LogBasic, Log, "[VPET BeginPlay] Lights: %i", m_state.numLights);
	DOL(LogBasic, Log, "[VPET BeginPlay] Cameras: %i", m_state.numCameras);

	// Open ØMQ context
	context = new zmq::context_t(1);

	// Prepare synchronization thread - TODO promote this to somewhere easier to find
	FString synchronizationPort(":5556");

	// Prepare ZMQ listener socket
	socket_s = new zmq::socket_t(*context, ZMQ_SUB);
	// cpp method
	socket_s->setsockopt(ZMQ_SUBSCRIBE, "", 0);
	// zmq function
	//int rc = zmq_setsockopt(socket_s, ZMQ_SUBSCRIBE, "", 0);
	//assert(rc == 0);

	try {
		FString hostAddress = FString("tcp://") + HostIP + synchronizationPort;
		std::string hostString = TCHAR_TO_UTF8(*hostAddress);
		socket_s->connect(hostString);
	}
	catch (const zmq::error_t &e)
	{
		FString errName = FString(zmq_strerror(e.num()));
		DOL(LogBasic, Error, "[VPET BeginPlay] ERROR - Failed to connect: %s", *errName);
		OSD(FColor::Red, "[VPET BeginPlay] ERROR - Failed to connect: %s", *errName);
		return;
	}

	DOL(LogBasic, Warning, "[VPET BeginPlay] Synchronization socket connected!");
	OSD(FColor::Cyan, "[VPET BeginPlay] Synchronization socket connected!");

	// Start the message queue?
	msgQ.clear();

	// Start synchronization (listener) thread
	auto tListener = new FAutoDeleteAsyncTask<ThreadSyncDev>(socket_s, &msgQ, LogBasic);
	tListener->StartBackgroundTask();

	// Host IP identifier
	int32 idIndex = INDEX_NONE;
	if (HostIP.FindLastChar('.', idIndex))
	{
		FString onlyID = HostIP.RightChop(idIndex + 1);
		m_id = FCString::Atoi(*onlyID);
	}
	DOL(LogBasic, Log, "[VPET BeginPlay] M_ID: %d", m_id);
	OSD(FColor::Cyan, "[VPET BeginPlay] M_ID: %d", m_id);


	// Prepare distribution thread - for hosting the scene - TODO promote this to somewhere easier to find
	FString distributionPort(":5565");
	socket_d = new zmq::socket_t(*context, ZMQ_REP);

	// Safe attempt to bind - if socket exists, stop it all
	try {
		FString hostAddress = FString("tcp://") + HostIP + distributionPort;
		std::string hostString(TCHAR_TO_UTF8(*hostAddress));
		socket_d->bind(hostString);

	}
	catch (const zmq::error_t &e)
	{
		FString errName = FString(zmq_strerror(e.num()));
		DOL(LogBasic, Error, "[VPET BeginPlay] ERROR - Failed to bind: %s", *errName);
		OSD(FColor::Red, "[VPET BeginPlay] ERROR - Failed to bind: %s", *errName);
		return;
	}

	DOL(LogBasic, Warning, "[VPET BeginPlay] Distribution socket created!");
	OSD(FColor::Cyan, "[VPET BeginPlay] Distribution socket created!");

	// Start distribution (request / reply) thread
	auto tDistribute = new FAutoDeleteAsyncTask<ThreadDistDev>(socket_d, &m_state, LogBasic);
	tDistribute->StartBackgroundTask();


}

void AVPETModule::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	// Process messages
	int count = 0;
	for (size_t i = 0; i < msgQ.size(); i++)
	{
		//uint8_t* msg = msgQ.at(i);
		//ParseTransformation(msg);
		// Vector paradigm
		std::vector<uint8_t> msg = msgQ.at(i);
		ParseTransformation(msg);
		count++;
	}

	// Clean processed messages
	msgQ.erase(msgQ.begin(), msgQ.begin() + count);

	// Development print
	if (doItOnce)
	{
		doItOnce = false;
		DOL(LogBasic, Log, "[VPET Once] Editable actors list:");
		OSD(FColor::Turquoise, "[VPET List] Editable actors list:");
		for (size_t i = 0; i < actorList.Num(); i++)
		{
			// Grab back the actors
			AActor* aActor = actorList[i];
			FString aName = aActor->GetActorLabel();
			DOL(LogBasic, Log, "[VPET] %d: %s", i, *aName);
			OSD(FColor::Turquoise, "[VPET List] %d: %s", i, *aName);
		}

		// checking instancing
		int objCount = m_state.objPackList.size();
		DOL(LogBasic, Log, "[VPET List] Unique object count: %d", objCount);
	}

}


void AVPETModule::ParseTransformation(std::vector<uint8_t> kMsg)
{
	//// Grab first byte
	//uint8_t fByte;
	////DOL(LogBasic, Log, "[Parse] First byte: %d", fByte);
	//// Development print - print a bunch of bytes
	//for (size_t i = 0; i < 30; i++)
	//{
	//	fByte = kMsg[i];
	//	DOL(LogBasic, Log, "[Parse] Byte: %d, value: %d", i, fByte);
	//}

	// Ignore message from host
	if (kMsg[0] != m_id)
	{
		ParameterType paramType = (ParameterType)kMsg[1];

		int32_t objectID = *reinterpret_cast<int32_t*>(&kMsg[2]);
		DOL(LogBasic, Log, "[SYNC Parse] obj Id: %d", objectID);
		OSD(FColor::Yellow, "[SYNC Parse] obj Id: %d", objectID);

		if (actorList.Num() <= objectID)
		{
			DOL(LogBasic, Error, "[SYNC Parse] Failed to grab object refered by Id: %d", objectID);
			OSD(FColor::Red, "[SYNC Parse] Failed to grab object refered by Id: %d", objectID);
			return;
		}

		AActor* sceneActor = actorList[objectID];

		switch (paramType)
		{
		case AVPETModule::POS:
		{
			float lX = *reinterpret_cast<float*>(&kMsg[6]);
			float lY = *reinterpret_cast<float*>(&kMsg[10]);
			float lZ = *reinterpret_cast<float*>(&kMsg[14]);
			DOL(LogBasic, Log, "[SYNC Parse] Type POS: %f %f %f", lX, lY, lZ);

			// Transform actor pos
			FVector aLoc(-lX, lZ, lY);

			aLoc *= 100.0;
			//sceneActor->SetActorLocation(aLoc);
			sceneActor->SetActorRelativeLocation(aLoc);

			break;
		}
		case AVPETModule::ROT:
		{
			float lX = *reinterpret_cast<float*>(&kMsg[6]);
			float lY = *reinterpret_cast<float*>(&kMsg[10]);
			float lZ = *reinterpret_cast<float*>(&kMsg[14]);
			float lW = *reinterpret_cast<float*>(&kMsg[18]);
			DOL(LogBasic, Log, "[SYNC Parse] Type ROT: %f %f %f %f", lX, lY, lZ, lW);

			// Transform actor rot
			FQuat aRot(-lX, lZ, lY, lW);

			// In the case of cameras and lights and maybe some other stuff that needs tweak (maybe could be transmitted from scene distribution as a bool already - no double chek)
			FString className = sceneActor->GetClass()->GetName();
			//DOL(LogBasic, Log, "[SYNC Parse] Class %s", *className);
			if (className.Find("Light") > -1 || className == "CameraActor")
			{
				//DOL(LogBasic, Log, "[SYNC Parse] Tweak rot!");
				FRotator tempRot(0, 90, 0);
				FQuat transRot = tempRot.Quaternion();
				aRot *= transRot;
			}

			//sceneActor->SetActorRotation(aRot);
			sceneActor->SetActorRelativeRotation(aRot);

			break;
		}
		case AVPETModule::SCALE:
		{
			float lX = *reinterpret_cast<float*>(&kMsg[6]);
			float lY = *reinterpret_cast<float*>(&kMsg[10]);
			float lZ = *reinterpret_cast<float*>(&kMsg[14]);
			DOL(LogBasic, Log, "[SYNC Parse] Type SCALE: %f %f %f", lX, lY, lZ);

			// transform actor sca
			FVector aSca(lX, lZ, lY);

			//sceneActor->SetActorScale3D(aSca);
			sceneActor->SetActorRelativeScale3D(aSca);

			break;
		}

		case AVPETModule::LOCK:
			DOL(LogBasic, Log, "[SYNC Parse] LOCK paramType");
			break;
		case AVPETModule::HIDDENLOCK:
			DOL(LogBasic, Log, "[SYNC Parse] HIDDENLOCK paramType");
			break;
		case AVPETModule::KINEMATIC:
			DOL(LogBasic, Log, "[SYNC Parse] KINEMATIC paramType");
			break;
		case AVPETModule::FOV:
		{
			DOL(LogBasic, Log, "[SYNC Parse] FOV paramType");
			ACameraActor* kCam = Cast<ACameraActor>(sceneActor);
			if (kCam)
				kCam->GetCameraComponent()->FieldOfView = *reinterpret_cast<float*>(&kMsg[6]);
			break;
		}
		case AVPETModule::ASPECT:
			DOL(LogBasic, Log, "[SYNC Parse] ASPECT paramType");
			break;
		case AVPETModule::FOCUSDIST:
			DOL(LogBasic, Log, "[SYNC Parse] FOCUSDIST paramType");
			break;
		case AVPETModule::FOCUSSIZE:
			DOL(LogBasic, Log, "[SYNC Parse] FOCUSSIZE paramType");
			break;
		case AVPETModule::APERTURE:
			DOL(LogBasic, Log, "[SYNC Parse] APERTURE paramType");
			break;
		case AVPETModule::COLOR:
		{
			DOL(LogBasic, Log, "[SYNC Parse] COLOR paramType");
			ALight* kLit = Cast<ALight>(sceneActor);
			if (kLit)
			{
				float lR = *reinterpret_cast<float*>(&kMsg[6]);
				float lG = *reinterpret_cast<float*>(&kMsg[10]);
				float lB = *reinterpret_cast<float*>(&kMsg[14]);
				kLit->SetLightColor(FLinearColor(lR, lG, lB));
			}
			break;
		}
		case AVPETModule::INTENSITY:
		{
			DOL(LogBasic, Log, "[SYNC Parse] INTENSITY paramType");
			ALight* kLit = Cast<ALight>(sceneActor);
			if (kLit)
			{
				float lB = *reinterpret_cast<float*>(&kMsg[6]);
				float lightFactor = 0.2;
				kLit->SetBrightness(lB / lightFactor / BrightnessMultiplier);
			}
			break;
		}
		case AVPETModule::EXPOSURE:
			DOL(LogBasic, Log, "[SYNC Parse] EXPOSURE paramType");
			break;
		case AVPETModule::RANGE:
		{
			DOL(LogBasic, Log, "[SYNC Parse] RANGE paramType");
			float lR = *reinterpret_cast<float*>(&kMsg[6]);
			float rangeFactor = 0.005;
			APointLight* kPointLgt = Cast<APointLight>(sceneActor);
			if (kPointLgt)
			{
				UPointLightComponent* pointLgtCmp = kPointLgt->PointLightComponent;
				pointLgtCmp->SetAttenuationRadius(lR / rangeFactor / RangeMultiplier);
			}
			ASpotLight* kSpotLgt = Cast<ASpotLight>(sceneActor);
			if (kSpotLgt)
			{
				USpotLightComponent* spotLgtCmp = kSpotLgt->SpotLightComponent;
				spotLgtCmp->SetAttenuationRadius(lR / rangeFactor / RangeMultiplier);
			}
			break;
		}
		case AVPETModule::ANGLE:
		{
			DOL(LogBasic, Log, "[SYNC Parse] ANGLE paramType");
			ASpotLight* kSpotLgt = Cast<ASpotLight>(sceneActor);
			if (kSpotLgt)
			{
				float lA = *reinterpret_cast<float*>(&kMsg[6]);
				USpotLightComponent* spotLgtCmp = kSpotLgt->SpotLightComponent;
				float coneFactor = 2.0;
				spotLgtCmp->SetOuterConeAngle(lA / coneFactor);
			}
			break;
		}
		case AVPETModule::BONEANIM:
			DOL(LogBasic, Log, "[SYNC Parse] BONEANIM paramType");
			break;
		case AVPETModule::VERTEXANIM:
			DOL(LogBasic, Log, "[SYNC Parse] VERTEXANIM paramType");
			break;
		case AVPETModule::PING:
			DOL(LogBasic, Log, "[SYNC Parse] PING paramType");
			break;
		case AVPETModule::RESENDUPDATE:
			DOL(LogBasic, Log, "[SYNC Parse] RESENDUPDATE paramType");
			break;
		case AVPETModule::CHARACTERTARGET:
			DOL(LogBasic, Log, "[SYNC Parse] CHARACTERTARGET paramType");
			break;
		default:
			DOL(LogBasic, Log, "[SYNC Parse] Unknown paramType");
			return;
			break;
		}
	}
}




// Perhaps temporary root construction
void AVPETModule::buildWorld()
{
	// Create an empty root
	std::string sName("root");
	m_state.node = new Node();
	m_state.nodeTypeList.push_back(NodeType::GROUP);
	DOL(LogBasic, Log, "[DIST buildWorld] Create root node ");
	// Assign name
	sName = sName.substr(0, 63);
	strcpy_s(m_state.node->name, sName.c_str());
	// zero it all
	m_state.node->position[0] = 0;
	m_state.node->position[1] = 0;
	m_state.node->position[2] = 0;
	m_state.node->rotation[0] = 0;
	m_state.node->rotation[1] = 0;
	m_state.node->rotation[2] = 0;
	m_state.node->rotation[3] = 0;
	m_state.node->scale[0] = 1;
	m_state.node->scale[1] = 1;
	m_state.node->scale[2] = 1;
	// This needs to be altered after actor iterating
	m_state.node->childCount = 0;
	// push root
	DOL(LogBasic, Log, "[DIST buildWorld] Added root Node");
	m_state.nodeList.push_back(m_state.node);
	m_state.node->editable = false;
}

bool AVPETModule::buildLocation(AActor *prim)
{
	// TODO
	// check LOD or equivalent tag

	FString aName = prim->GetActorLabel();
	//DOL(LogBasic, Log, "[DIST buildLocation] Build: %s", *aName);

	// Temp var for camera and light tweak
	bool tweakRot = false;

	// Get class type
	FString className = prim->GetClass()->GetName();


	// If using tag system for selecting what to send
	if (UseSendTag)
	{
		if (!prim->ActorHasTag(TEXT("send")))
		{
			DOL(LogBasic, Log, "[DIST buildLocation] Not sending: %s; class: %s", *aName, *className);
			return false;
		}
	}


	// HACK - skip spawn camera
	if (prim->GetName().Compare(FString("CameraActor_0")) == 0)
		return false;

	// Skip tag "skip" 
	if (prim->ActorHasTag(TEXT("skip")))
	{
		DOL(LogBasic, Log, "[DIST buildLocation] Skipped build: %s; class: %s", *aName, *className);
		return false;
	}


	// tag print - development test
	TArray<FName> pTags = prim->Tags;
	DOL(LogTag, Warning, "[DIST buildLocation] Actor %s has %d tags!", *aName, pTags.Num());
	for (size_t i = 0; i < pTags.Num(); i++)
	{
		DOL(LogTag, Log, "[DIST buildLocation] Tag %d: %s", i, *pTags[i].ToString());
	}


	m_state.node = 0;

	//if (className == "StaticMeshActor") {
	if (className == "StaticMeshActor" || prim->ActorHasTag(TEXT("mesh"))) {
		m_state.node = new NodeGeo();
		buildNode((NodeGeo*)m_state.node, prim);
		DOL(LogBasic, Log, "[DIST buildLocation] Found geo actor, creating geo node out of %s; class: %s", *aName, *className);
	}
	else if (className == "CameraActor") {
		m_state.node = new NodeCam();
		buildNode((NodeCam*)m_state.node, prim);
		DOL(LogBasic, Log, "[DIST buildLocation] Found camera actor, creating camera node out of %s; class: %s", *aName, *className);
		tweakRot = true;
	}
	//else if (className.Find("Light") > -1) {
	//else if (className.Find("DirectionalLight") > -1) {
	else if (className.Find("Light") > -1) {
		if (className.Find("Directional") > -1 || className.Find("Spot") > -1 || className.Find("Point") > -1)
		{
			m_state.node = new NodeLight();
			buildNode((NodeLight*)m_state.node, prim, className);
			DOL(LogBasic, Log, "[DIST buildLocation] Found light actor, creating light node out of %s; class: %s", *aName, *className);
			tweakRot = true;
		}
	}
	else
	{
		m_state.node = new Node();
		m_state.nodeTypeList.push_back(NodeType::GROUP);
		DOL(LogBasic, Log, "[DIST buildLocation] Found unknown actor, creating group node out of %s; class: %s", *aName, *className);
	}

	if (!m_state.node)
	{
		DOL(LogBasic, Log, "[DIST buildLocation] Discarded build: %s; class: %s", *aName, *className);
		return false;
	}

	// Short name
	std::string sName = TCHAR_TO_UTF8(*aName);
	sName = sName.substr(0, 63);
	// Assign name
	strcpy_s(m_state.node->name, sName.c_str());

	// Grab transform: pos rot sca
	FTransform aTrans = prim->GetActorTransform();
	//// vs? falls back to identity
	//FTransform gTrans = prim->GetTransform();

	// use relative transform if has parent
	if (prim->GetAttachParentActor())
		aTrans = UKismetMathLibrary::MakeRelativeTransform(aTrans, prim->GetAttachParentActor()->GetActorTransform());

	// Decompose
	FVector aPos = aTrans.GetTranslation();
	FQuat aRot = aTrans.GetRotation();
	FVector aSca = aTrans.GetScale3D();

	// Scale position values
	aPos *= .01;

	// Tweak rotation if it is light
	if (tweakRot)
	{
		FRotator tempRot(0, -90, 0);
		FQuat transRot = tempRot.Quaternion();
		aRot *= transRot;
	}


	// Alter rotations
	FQuat modRot(-aRot.X, aRot.Z, aRot.Y, aRot.W);
	aRot = modRot;

	// Set data - mod
	m_state.node->position[0] = -aPos.X;
	m_state.node->position[1] = aPos.Z;
	m_state.node->position[2] = aPos.Y;

	//DOL(LogBasic, Log, "Node pos: %f, %f, %f", m_state.node->position[0], m_state.node->position[1], m_state.node->position[2]);

	m_state.node->rotation[0] = aRot.X;
	m_state.node->rotation[1] = aRot.Y;
	m_state.node->rotation[2] = aRot.Z;
	m_state.node->rotation[3] = aRot.W;

	//DOL(LogBasic, Log, "Node rot: %f, %f, %f, %f", m_state.node->rotation[0], m_state.node->rotation[1], m_state.node->rotation[2], m_state.node->rotation[3]);

	//m_state.node->scale[0] = aSca.X * .01;
	//m_state.node->scale[1] = aSca.Y * .01;
	//m_state.node->scale[2] = aSca.Z * .01;
	m_state.node->scale[0] = aSca.X;
	m_state.node->scale[1] = aSca.Z;
	m_state.node->scale[2] = aSca.Y;

	//DOL(LogBasic, Log, "Node sca: %f, %f, %f", m_state.node->scale[0], m_state.node->scale[1], m_state.node->scale[2]);

	// Prepare children counter
	int childCount = 0;
	m_state.node->childCount = 0;

	// Grab attached actors
	TArray<AActor*> attActors;
	prim->GetAttachedActors(attActors);

	// Print name back
	FString nName(m_state.node->name);
	DOL(LogBasic, Warning, "[DIST buildLocation] Added Node: %s", *nName);
	m_state.nodeList.push_back(m_state.node);


	// ideally temporary indexing workaround (to be able to tweak properties of this node later)
	int nodeIndex = m_state.nodeList.size() - 1;

	// Editable or not

	// Should use tag system?
	if (UseEditableTag)
	{
		m_state.node->editable = false;
		// check if has tag
		//if (prim->ActorHasTag(TEXT("editable")))

		// HACK - if light or camera - also always editable
		if (prim->ActorHasTag(TEXT("editable")) || tweakRot)
		{
			//DOL(LogBasic, Warning, "[DIST buildLocation] has editable tag!");
			m_state.node->editable = true;

			// List it to be fetched at sync
			AddActorPointer(prim);

			// Warn in case is not movable
			if (!prim->IsRootComponentMovable())
			{
				DOL(LogBasic, Warning, "[DIST buildLocation] Actor %s is set to editable but is not movable! This might lead to issues.", *nName);
				OSD(FColor::Orange, "[DIST buildLocation] Actor %s is set to editable but is not movable! This might lead to issues.", *nName);
			}
		}
	}
	else
	{
		// as long as movable, set to editable
		m_state.node->editable = prim->IsRootComponentMovable();
		AddActorPointer(prim);
	}

	// Development counter hack - add sub group for counter-rotating children, if it has any
	if (tweakRot && attActors.Num() > 0)
	{
		m_state.nodeList.at(nodeIndex)->childCount = 1;
		buildEmptyRotator(nName);
		//return true;
		// point out that the empty will be the father
		nodeIndex++;
	}

	// Recurse to "children" 
	//// temporary HACK - don't do it if camera or light, unless instructed to do so with bool
	if (SendLightChild || !tweakRot)
	{
		for (size_t i = 0; i < attActors.Num(); i++)
		{
			// only count child if it was build
			if (buildLocation(attActors[i]))
				childCount++;
		}
	}

	// After iteration, update children count
	m_state.nodeList.at(nodeIndex)->childCount = childCount;

	/*
	// development check
	DOL(LogBasic, Warning, "[DIST buildLocation] Local node %s has %d children", *nName, childCount);

	FString lName(m_state.nodeList.at(nodeIndex)->name);
	DOL(LogBasic, Warning, "[DIST buildLocation] List node %s has %d children", *lName, m_state.nodeList.at(nodeIndex)->childCount);
	*/

	return true;

}


void AVPETModule::buildEmptyRotator(FString parentName)
{
	FString aName(parentName + "_flipRot");
	m_state.node = new Node();
	m_state.nodeTypeList.push_back(NodeType::GROUP);
	std::string sName = TCHAR_TO_UTF8(*aName);
	sName = sName.substr(0, 63);
	strcpy_s(m_state.node->name, sName.c_str());
	// zero position
	m_state.node->position[0] = 0;
	m_state.node->position[1] = 0;
	m_state.node->position[2] = 0;
	// counter rotation
	FRotator tempRot(0, 90, 0);
	FQuat counterRot = tempRot.Quaternion();
	FQuat aRot(-counterRot.X, counterRot.Z, counterRot.Y, counterRot.W);
	m_state.node->rotation[0] = aRot.X;
	m_state.node->rotation[1] = aRot.Y;
	m_state.node->rotation[2] = aRot.Z;
	m_state.node->rotation[3] = aRot.W;
	// unitary scale
	m_state.node->scale[0] = 1;
	m_state.node->scale[1] = 1;
	m_state.node->scale[2] = 1;
	// this will be adjusted after children iterating
	m_state.node->childCount = 0;
	// push counter rotation node
	DOL(LogBasic, Log, "[buildEmptyRotator] Added counter rotator: %s", *aName);
	m_state.nodeList.push_back(m_state.node);
	m_state.node->editable = false;
}


void AVPETModule::buildNode(NodeGeo *node, AActor* prim)
{
	m_state.node = node;
	m_state.nodeTypeList.push_back(NodeType::GEO);

	std::string instanceID = "";


	// Get mesh components
	TArray<UStaticMeshComponent*> staticMeshComponents;
	prim->GetComponents<UStaticMeshComponent>(staticMeshComponents);
	// Test if got mesh
	if (staticMeshComponents.Num() == 0) {
		DOL(LogBasic, Log, "[DIST buildNode] No mesh found!");
		return;
	}

	// Grab only the first
	UStaticMeshComponent* staticMeshComponent = staticMeshComponents[0];
	UStaticMesh* staticMesh = staticMeshComponent->GetStaticMesh();

	// Grab path of static mesh as mesh ID
	FString pName = staticMesh->GetPathName();
	//DOL(LogBasic, Log, "[DIST buildNode] instanceID: %s", *pName);

	instanceID = TCHAR_TO_UTF8(*pName);
	int i = 0;
	for (; i < m_state.objPackList.size(); ++i)
	{
		if (m_state.objPackList[i].instanceId == instanceID)
		{
			break;
		}
	}
	if (i < m_state.objPackList.size())
	{
		node->geoId = i;
		//DOL(LogBasic, Log, "[DIST buildNode] Instantiate to: %d", node->geoId);
	}

	if (node->geoId < 0)
	{
		// Create Package
		ObjectPackage objPack;
		objPack.instanceId = instanceID;

		// Vertices / Points
		//staticMesh->GetNumVertices(0);
		//DOL(LogBasic, Log, "[DIST Build] Vertex Count 1 : %d", staticMesh->GetNumVertices(0));

		TArray<FVector> vertices = TArray<FVector>();

		// Validity checks?
		if (!prim->IsValidLowLevel()) return;
		if (!staticMeshComponent) return;;
		if (!staticMesh) return;
		if (!staticMesh->RenderData) return;

		if (staticMesh->RenderData->LODResources.Num() > 0)
		{
			FStaticMeshLODResources &resource = staticMesh->RenderData->LODResources[0];
			// Index Buffer
			FRawStaticIndexBuffer &ib = resource.IndexBuffer;
			// TODO - does it need to check if(&ib)?

			// Build indices list - in reverse order, because why not?
			//for (size_t j = ib.GetNumIndices(); j > 0 ; j--) {
			//	// Push to pack
			//	objPack.indices.push_back(ib.GetIndex(j-1));
			//}
			//// Use this instead?
			FIndexArrayView arrV = ib.GetArrayView();
			for (size_t j = arrV.Num(); j > 0; j--) {
				objPack.indices.push_back(arrV[j - 1]);
			}

			// Vertex Buffer
			FStaticMeshVertexBuffers &vbs = resource.VertexBuffers;
			// Color VertexBuffer
			//FColorVertexBuffer &colVP = vbs.ColorVertexBuffer;
			//for (size_t j = 0; j < colVP.GetNumVertices(); j++) {
			//	DOL(LogBasic, Log, "i: %d - VCol: %f, %f, %f", j, colVP.VertexColor(j).R, colVP.VertexColor(j).G, colVP.VertexColor(j).B);
			//}
			// Position VertexBuffer
			FPositionVertexBuffer &posVP = vbs.PositionVertexBuffer;
			for (size_t j = 0; j < posVP.GetNumVertices(); j++) {
				//DOL(LogBasic, Log, "i: %d - VPos: %f, %f, %f", j, posVP.VertexPosition(j).X, posVP.VertexPosition(j).Y, posVP.VertexPosition(j).Z);
				// Push to pack
				objPack.vertices.push_back(-posVP.VertexPosition(j).X*0.01);
				objPack.vertices.push_back(posVP.VertexPosition(j).Z*0.01);
				objPack.vertices.push_back(posVP.VertexPosition(j).Y*0.01);
			}
			// Mesh vertex buffer
			FStaticMeshVertexBuffer &smVP = vbs.StaticMeshVertexBuffer;
			for (size_t j = 0; j < smVP.GetNumVertices(); j++) {
				//DOL(LogBasic, Log, "i: %d - VNormal: %f, %f, %f", j, smVP.VertexTangentZ(j).X, smVP.VertexTangentZ(j).Y, smVP.VertexTangentZ(j).Z);
				// Push to pack
				objPack.normals.push_back(-smVP.VertexTangentZ(j).X);
				objPack.normals.push_back(smVP.VertexTangentZ(j).Z);
				objPack.normals.push_back(smVP.VertexTangentZ(j).Y);
				FVector2D vUV = smVP.GetVertexUV(j, 0);
				//DOL(LogBasic, Log, "i: %d - UV: %f, %f", j, vUV.X, vUV.Y);
				// Push to pack
				objPack.uvs.push_back(vUV.X);
				objPack.uvs.push_back(vUV.Y);
			}

			// Print 
			DOL(LogGeoBuild, Log, "[DIST buildNode] Build information of mesh %s:", *pName);
			DOL(LogGeoBuild, Log, "[DIST buildNode] Vertex Count: %d, Normal Count: %d, Index Count: %d", int(objPack.vertices.size() / 3.0), int(objPack.normals.size() / 3.0), objPack.indices.size());

			// store the object package
			m_state.objPackList.push_back(objPack);

			// get geo id
			node->geoId = m_state.objPackList.size() - 1;
		}
	} // if ( nodeGeo->geoId < 0 )


	// Grab first material
	//UMaterialInterface* sMat = staticMesh->GetMaterial(0); // this is the material of the static mesh itself
	UMaterialInterface* cMat = staticMeshComponent->GetMaterial(0);

	DOL(LogMaterial, Warning, "[DIST buildNode] num mat: %d", staticMeshComponent->GetNumMaterials());
	//if (sMat)
	//	DOL(LogBasic, Log, "[DIST buildNode] sMat: %s", *sMat->GetName());
	if (cMat)
		DOL(LogMaterial, Log, "[DIST buildNode] cMat: %s", *cMat->GetName());
	//prim->GetComponents<material>(staticMeshComponents);


	// this returns the parent of the material (e.g. case of material instance)
	//UMaterial* ccMat = cMat->GetMaterial();
	//if (ccMat)
	//	DOL(LogBasic, Log, "[DIST buildNode] ccMat: %s", *ccMat->GetName());

	// So trying to get data from it is pointless
	//FColorMaterialInput cbColor = ccMat->BaseColor;
	//FColor cbCons = cbColor.Constant;
	//DOL(LogBasic, Warning, "[DIST buildNode] base color: %d %d %d", cbCons.R, cbCons.G, cbCons.B);

	// Using texture?
	bool kHasTexture = cMat->HasTextureStreamingData();
	if (kHasTexture)
	{
		//DOL(LogBasic, Warning, "[DIST buildNode] material HAS texture streaming data");

		//TArray<UTexture*> kTextures;
		//TArray<TArray<int32>> kIndices;
		//cMat->GetUsedTexturesAndIndices(kTextures, kIndices, EMaterialQualityLevel::Medium, ERHIFeatureLevel::ES2_REMOVED);

		//DOL(LogBasic, Warning, "[DIST buildNode] length texture array: %d", kTextures.Num());
		//DOL(LogBasic, Warning, "[DIST buildNode] length indices array: %d", kIndices.Num());

		TArray<UTexture*> kUTextures;
		//cMat->GetUsedTextures(kUTextures, EMaterialQualityLevel::Medium, true, ERHIFeatureLevel::ES2_REMOVED, true); // >= 4.25
		cMat->GetUsedTextures(kUTextures, EMaterialQualityLevel::Medium, true, ERHIFeatureLevel::Num, true);
		DOL(LogMaterial, Warning, "[DIST buildNode] length texture array: %d", kUTextures.Num());

		//for (size_t j = 0; j < kUTextures.Num(); j++)
		size_t j = 0;
		for (UTexture* kTex : kUTextures)
		{
			//UTexture* kTex = kUTextures[j];
			float kBri = kTex->GetAverageBrightness(false, false);
			DOL(LogMaterial, Warning, "texture %d, named %s, brig: %f", j, *kTex->GetName(), kBri);
			j++;
		}
	}

	// shading model? any useful?
	//FMaterialShadingModelField cShad = cMat->GetShadingModels();
	//if (cShad.IsValid())
	//	DOL(LogBasic, Warning, "[DIST buildNode] material HAS shading model");


	// all textures?
	TArray<FMaterialTextureInfo> texStr = cMat->GetTextureStreamingData();
	for (size_t j = 0; j < texStr.Num(); j++)
	{
		FMaterialTextureInfo kTex = texStr[j];
		DOL(LogMaterial, Warning, "streaming data texture %d named: %s", j, *kTex.TextureName.ToString());
	}


	// width? - nope - unresolved function! - will not compile
	//int kWid = cMat->GetWidth();
	//DOL(LogBasic, Warning, "width %d");


	// Get base material that is being instanced
	UMaterial* cbMat = cMat->GetBaseMaterial();
	if (cbMat)
	{

		DOL(LogMaterial, Log, "[DIST buildNode] cbMat: %s", *cbMat->GetName());

		// Get base color input
		FColorMaterialInput cbColor = cbMat->BaseColor;
		if (cbColor.IsConnected())
		{
			UMaterialExpression* cbExpr = cbColor.Expression;
			DOL(LogMaterial, Log, "Expression get name: %s", *cbExpr->GetName());
			DOL(LogMaterial, Log, "Expression description: %s", *cbExpr->GetDescription());

			// This crashes
			//DOL(LogBasic, Warning, "Expression editable name: %s", *cbExpr->GetEditableName());

			// Casting experiments
			auto exParam = dynamic_cast<UMaterialExpressionParameter*>(cbExpr);
			auto exConst3 = dynamic_cast<UMaterialExpressionConstant3Vector*>(cbExpr);
			auto exConst4 = dynamic_cast<UMaterialExpressionConstant4Vector*>(cbExpr);
			auto exTextu = dynamic_cast<UMaterialExpressionTextureBase*>(cbExpr);

			// Parameter
			if (exParam != NULL)
			{
				// probably redundant since there is cbExpr->bIsParameterExpression
				DOL(LogMaterial, Warning, "Cast to parameter ok!");
				DOL(LogMaterial, Warning, "Expression has parameter: %s", *cbExpr->GetParameterName().ToString());

				// The parameter value (as exposed in GetDescription()) does not represent the material instance parameter, but the source material instead

				FName matParam = cbExpr->GetParameterName();

				// Hack - using a parameter to be able to transmit color to node
				// This conditional is not really needed since the GetVectorParameterValue will only be true if it already checks
				if (cbExpr->GetParameterName().Compare(matParam) == 0)
				{
					FLinearColor paramColor;
					//TEXT("kColor")

					/*
									if (cbExpr->GetParameterName().Compare(FName("kColor")) == 0)
				{
					FLinearColor kColor;
					if (cMat->GetVectorParameterValue(TEXT("kColor"), kColor))

					*/
					//if (cMat->GetVectorParameterValue(matParam), paramColor)
					if (cMat->GetVectorParameterValue(matParam, paramColor))
					{
						// Custom color attribute
						node->color[0] = paramColor.R;
						node->color[1] = paramColor.G;
						node->color[2] = paramColor.B;
						DOL(LogMaterial, Warning, "[DIST buildNode] color: %f %f %f", paramColor.R, paramColor.G, paramColor.B);

					}
				}

			}
			else if (exConst3 != NULL)
			{
				DOL(LogMaterial, Warning, "Cast to constant 3 ok!");
				FLinearColor cColor = exConst3->Constant;

				node->color[0] = cColor.R;
				node->color[1] = cColor.G;
				node->color[2] = cColor.B;
			}
			else if (exConst4 != NULL)
			{
				DOL(LogMaterial, Warning, "Cast to constant 4 ok!");
				FLinearColor cColor = exConst4->Constant;

				node->color[0] = cColor.R;
				node->color[1] = cColor.G;
				node->color[2] = cColor.B;
			}
			else if (exTextu != NULL)
			{
				DOL(LogMaterial, Warning, "Cast to texture ok!");
				// See if has texture
				UObject* refTex = cbExpr->GetReferencedTexture();
				// UObject* refTex = exTextu->GetReferencedTexture();
				// This will only return valid if the texture is directly connected to the base color input
				if (refTex)
					DOL(LogMaterial, Warning, "text name %s", *refTex->GetName());

			}

		}
		else
		{
			DOL(LogMaterial, Warning, "Base color not connected");
			// Color in unreal would be black
			node->color[0] = 0;
			node->color[1] = 0;
			node->color[2] = 0;
		}

	}

	// store at sharedState to access it in iterator
	m_state.node = node;
	m_state.numObjectNodes++;

}

void AVPETModule::buildNode(NodeCam *node, AActor* prim)
{
	m_state.node = node;
	m_state.nodeTypeList.push_back(NodeType::CAMERA);

	// Grab camera
	ACameraActor* kCam = Cast<ACameraActor>(prim);
	node->cFov = kCam->GetCameraComponent()->FieldOfView;

	// magic number tests
	node->cNear = 0.001;
	node->cFar = 100;;
	//node->editable = true;

	// store at sharedState to access it in iterator
	m_state.node = node;
	m_state.numCameras++;
}

void AVPETModule::buildNode(NodeLight *node, AActor* prim, FString className)
{
	m_state.node = node;
	m_state.nodeTypeList.push_back(NodeType::LIGHT);

	node->type = VPET::DIRECTIONAL;

	ALight* kLgt = Cast<ALight>(prim);


	// Type

	if (className.Find("Directional") > -1) {
		node->type = VPET::DIRECTIONAL;
	}
	else if (className.Find("Point") > -1) {
		node->type = VPET::POINT;
		APointLight* kPointLgt = Cast<APointLight>(kLgt);
		if (kPointLgt)
		{
			UPointLightComponent* pointLgtCmp = kPointLgt->PointLightComponent;

			// Range <-> attenuation radius scaled down 
			float rangeFactor = 0.005;
			node->range = pointLgtCmp->AttenuationRadius * rangeFactor * RangeMultiplier;
			//DOL(LogBasic, Warning, "[DIST buildNode] light attenuation: %f", pointLgtCmp->AttenuationRadius);
			//pointLgtCmp->SetAttenuationRadius(50);
		}
	}
	else if (className.Find("Spot") > -1) {
		node->type = VPET::SPOT;

		ASpotLight* kSpotLgt = Cast<ASpotLight>(kLgt);
		if (kSpotLgt)
		{
			USpotLightComponent* spotLgtCmp = kSpotLgt->SpotLightComponent;

			// Angle <-> outer cone angle scaled up
			float coneFactor = 2.0;
			node->angle = spotLgtCmp->OuterConeAngle * coneFactor;
			// Range <-> attenuation radius scaled down 
			float rangeFactor = 0.005;
			node->range = spotLgtCmp->AttenuationRadius * rangeFactor * RangeMultiplier;
		}
	}
	else
	{
		// ignore unknown light for now - should actually never get here
		return;
	}

	FLinearColor lgtColor = kLgt->GetLightColor();
	node->color[0] = lgtColor.R;
	node->color[1] = lgtColor.G;
	node->color[2] = lgtColor.B;

	//DOL(LogBasic, Warning, "[DIST buildNode] light color: %f %f %f", lgtColor.R, lgtColor.G, lgtColor.B);

	float lgtBright = kLgt->GetBrightness();

	//DOL(LogBasic, Warning, "[DIST buildNode] light brightness: %f", lgtBright);

	// scale down intensity numbers
	float lightFactor = 0.2;
	node->intensity = lgtBright * lightFactor * BrightnessMultiplier;

	// exposure seems not to be used
	node->exposure = 1;

	// USD code
	/*
	UsdLuxLight light = UsdLuxLight(*prim);
	std::string typeName = prim->GetTypeName();

	GfVec3f color;
	light.GetColorAttr().Get(&color);
	node->color[0] = color[0];
	node->color[1] = color[1];
	node->color[2] = color[2];
	float intensity;
	light.GetIntensityAttr().Get(&node->intensity);
	float exposure;
	light.GetExposureAttr().Get(&node->exposure);

	if (typeName == "SphereLight") {
		node->type = VPET::POINT;
	}
	else if (typeName == "DistantLight") {
		node->type = VPET::DIRECTIONAL;
	}
	else if (typeName == "RectLight" || typeName == "DiscLight") {
		node->type = VPET::AREA;
		node->angle = 180;
	}
	else {
		UsdAttribute coneAngleAttr = prim->GetAttribute(UsdLuxTokens->shapingConeAngle);
		if (coneAngleAttr) {
			node->type = VPET::SPOT;
			float coneAgle;
			coneAngleAttr.Get(&node->angle);
		}
		else {
			node->type = VPET::NONE;
			delete node;
			Node* node = new Node();
			m_state.node = node;
			std::cout << "[DIST SceneDistributor.LightScenegraphLocationDelegate] Found unknown Light (add as group)" << std::endl;
			return;
		}
	}
	std::cout << "[DIST SceneDistributor.LightScenegraphLocationDelegate] Light color: " << node->color[0] << " " << node->color[1] << " " << node->color[2] << " Type: " << typeName << " intensity: " << node->intensity << " exposure: " << node->exposure << " coneAngle: " << node->angle << std::endl;
	*/
	// store at sharedState to access it in iterator
	m_state.node = node;
	m_state.numLights++;

}


void AVPETModule::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	Super::EndPlay(EndPlayReason);

	DOL(LogBasic, Log, "[VPET EndPlay] Game ended.");

	// Stop distribution thread
	DOL(LogBasic, Warning, "[VPET Endplay] Closing Zmq distribution socket...");
	if (socket_d)
		socket_d->close();
	delete socket_d;
	// Stop listener thread
	DOL(LogBasic, Warning, "[VPET Endplay] Closing Zmq synchronization socket...");
	if (socket_s)
		socket_s->close();
	delete socket_s;

	DOL(LogBasic, Warning, "[VPET Endplay] Destroying Zmq context...");
	if (context)
		context->close();
	delete context;

	DOL(LogBasic, Log, "[VPET Endplay] Endplay.");

}


// threads


// Synchronization thread
void ThreadSyncDev::DoWork()
{
	DOL(doLog, Warning, "[SYNC Thread] zeroMQ subscriber thread running");

	zmq::message_t message;
	std::string msgString;
	uint8_t* byteStream;
	std::vector<uint8_t> byteVector;

	std::vector<std::string> stringVect;

	while (1)
	{
		char* responseMessageContent = NULL;
		char* messageStart = NULL;
		int responseLength = 0;

		// Blocking receive
		try {
			socket->recv(&message);
		}
		catch (const zmq::error_t &e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[SYNC Thread] recv exception: %s", *errName);
			return;
		}

		const char* msgPointer = static_cast<const char*>(message.data());
		if (msgPointer == NULL) {
			DOL(doLog, Error, "[SYNC Thread] Error msgPointer is NULL");
		}
		else
		{
			// this reference gets lost
			//uint8_t* byteStream;
			//byteStream = static_cast<uint8_t*>(message.data()), message.size();
			//DOL(true, Warning, "byte stream 0 1 2 3 4 5 6 7: %d %d %d %d %d %d %d %d", byteStream[0], byteStream[1], byteStream[2], byteStream[3], byteStream[4], byteStream[5], byteStream[6], byteStream[7]);
			//msgQ->push_back(byteStream);

			// Shifting into std::vector
			byteVector.clear();
			byteStream = static_cast<uint8_t*>(message.data()), message.size();
			for (size_t i = 0; i < message.size(); i++)
			{
				byteVector.push_back(byteStream[i]);
			}
			msgQ->push_back(byteVector);
		}

		// Fallback for free-running while
		//Sleep(10);
	}
}


// Distribution thread
void ThreadDistDev::DoWork()
{
	DOL(doLog, Warning, "[DIST Thread] zeroMQ request-reply thread running");

	zmq::message_t message;
	std::string msgString;

	while (1)
	{
		char* responseMessageContent = NULL;
		char* messageStart = NULL;
		int responseLength = 0;

		// Blocking receive
		try {
			socket->recv(&message, 0);
		}
		catch (const zmq::error_t &e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[DIST Thread] recv exception: %s", *errName);
			return;
		}

		const char* msgPointer = static_cast<const char*>(message.data());
		if (msgPointer == NULL) {
			DOL(doLog, Error, "[DIST Thread] Error msgPointer is NULL");
		}
		else {
			msgString = std::string(static_cast<char*>(message.data()), message.size());
		}

		FString fString(msgString.c_str());
		DOL(doLog, Log, "[DIST Thread] Got request string: %s", *fString);

		// Header request
		if (msgString == "header")
		{
			DOL(doLog, Log, "[DIST Thread] Got Header Request");
			responseLength = sizeof(VpetHeader);
			messageStart = responseMessageContent = (char*)malloc(responseLength);
			memcpy(responseMessageContent, (char*)&(m_sharedState->vpetHeader), sizeof(VpetHeader));
		}
		// Materials request

		// Textures request
		else if (msgString == "textures")
		{
			//DOL(doLog, Log, );
			DOL(doLog, Log, "[DIST Thread] Got Textures Request");
			DOL(doLog, Log, "[DIST Thread] Texture count: %d", m_sharedState->texPackList.size());

			responseLength = sizeof(int) + sizeof(int)*m_sharedState->texPackList.size();
			for (int i = 0; i < m_sharedState->texPackList.size(); i++)
			{
				responseLength += m_sharedState->texPackList[i].colorMapDataSize;
			}

			messageStart = responseMessageContent = (char*)malloc(responseLength);

			// texture binary type (image data (0) or raw unity texture data (1))
			int textureBinaryType = m_sharedState->textureBinaryType;
			//std::cout << " textureBinaryType: " << textureBinaryType << std::endl;
			memcpy(responseMessageContent, (char*)&textureBinaryType, sizeof(int));
			responseMessageContent += sizeof(int);

			for (int i = 0; i < m_sharedState->texPackList.size(); i++)
			{
				memcpy(responseMessageContent, (char*)&m_sharedState->texPackList[i].colorMapDataSize, sizeof(int));
				responseMessageContent += sizeof(int);
				memcpy(responseMessageContent, m_sharedState->texPackList[i].colorMapData, m_sharedState->texPackList[i].colorMapDataSize);
				responseMessageContent += m_sharedState->texPackList[i].colorMapDataSize;
			}
		}

		// Objects request
		else if (msgString == "objects")
		{
			DOL(doLog, Log, "[DIST Thread] Got Objects Request");
			DOL(doLog, Log, "[DIST Thread] Object count: %d", m_sharedState->objPackList.size());

			responseLength = sizeof(int) * 5 * m_sharedState->objPackList.size();
			for (int i = 0; i < m_sharedState->objPackList.size(); i++)
			{
				responseLength += sizeof(float) * m_sharedState->objPackList[i].vertices.size();
				responseLength += sizeof(int) * m_sharedState->objPackList[i].indices.size();
				responseLength += sizeof(float) * m_sharedState->objPackList[i].normals.size();
				responseLength += sizeof(float) * m_sharedState->objPackList[i].uvs.size();
				responseLength += sizeof(float) * m_sharedState->objPackList[i].boneWeights.size();
				responseLength += sizeof(int) * m_sharedState->objPackList[i].boneIndices.size();
			}

			messageStart = responseMessageContent = (char*)malloc(responseLength);

			for (int i = 0; i < m_sharedState->objPackList.size(); i++)
			{
				// vSize
				int numValues = m_sharedState->objPackList[i].vertices.size() / 3.0;
				memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
				responseMessageContent += sizeof(int);
				// vertices
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].vertices[0], sizeof(float) * m_sharedState->objPackList[i].vertices.size());
				responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].vertices.size();
				// iSize
				numValues = m_sharedState->objPackList[i].indices.size();
				memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
				responseMessageContent += sizeof(int);
				// indices
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].indices[0], sizeof(int) * m_sharedState->objPackList[i].indices.size());
				responseMessageContent += sizeof(int) * m_sharedState->objPackList[i].indices.size();
				// nSize
				numValues = m_sharedState->objPackList[i].normals.size() / 3.0;
				memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
				responseMessageContent += sizeof(int);
				// normals
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].normals[0], sizeof(float) * m_sharedState->objPackList[i].normals.size());
				responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].normals.size();
				// uSize
				numValues = m_sharedState->objPackList[i].uvs.size() / 2.0;
				memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
				responseMessageContent += sizeof(int);
				// uvs
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].uvs[0], sizeof(float) * m_sharedState->objPackList[i].uvs.size());
				responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].uvs.size();
				// bWSize
				numValues = m_sharedState->objPackList[i].boneWeights.size() / 4.0;
				memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
				responseMessageContent += sizeof(int);
				// bone Weights
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].boneWeights[0], sizeof(float) * m_sharedState->objPackList[i].boneWeights.size());
				responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].boneWeights.size();
				// bone Indices
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].boneIndices[0], sizeof(int) * m_sharedState->objPackList[i].boneIndices.size());
				responseMessageContent += sizeof(int) * m_sharedState->objPackList[i].boneIndices.size();
			}

		}

		// Nodes request
		else if (msgString == "nodes")
		{
			DOL(doLog, Log, "[DIST Thread] Got Nodes Request");
			DOL(doLog, Log, "[DIST Thread] Node count: %d; Node Type count: %d", m_sharedState->nodeList.size(), m_sharedState->nodeList.size());

			// set the size from type- and name length
			responseLength = sizeof(NodeType) * m_sharedState->nodeList.size();

			// extend with sizeof node depending on node type
			for (int i = 0; i < m_sharedState->nodeList.size(); i++)
			{

				if (m_sharedState->nodeTypeList[i] == NodeType::GEO)
					responseLength += sizeof_nodegeo;
				else if (m_sharedState->nodeTypeList[i] == NodeType::LIGHT)
					responseLength += sizeof_nodelight;
				else if (m_sharedState->nodeTypeList[i] == NodeType::CAMERA)
					responseLength += sizeof_nodecam;
				else
					responseLength += sizeof_node;

			}

			// allocate memory for out byte stream
			messageStart = responseMessageContent = (char*)malloc(responseLength);

			// iterate over node list copy data to out byte stream
			for (int i = 0; i < m_sharedState->nodeList.size(); i++)
			{
				Node* node = m_sharedState->nodeList[i];

				// First Copy node type
				int nodeType = m_sharedState->nodeTypeList[i];
				memcpy(responseMessageContent, (char*)&nodeType, sizeof(int));
				responseMessageContent += sizeof(int);

				// Copy specific node data
				if (m_sharedState->nodeTypeList[i] == NodeType::GEO)
				{
					memcpy(responseMessageContent, node, sizeof_nodegeo);
					responseMessageContent += sizeof_nodegeo;
				}
				else if (m_sharedState->nodeTypeList[i] == NodeType::LIGHT)
				{
					memcpy(responseMessageContent, node, sizeof_nodelight);
					responseMessageContent += sizeof_nodelight;
				}
				else if (m_sharedState->nodeTypeList[i] == NodeType::CAMERA)
				{
					memcpy(responseMessageContent, node, sizeof_nodecam);
					responseMessageContent += sizeof_nodecam;
				}
				else
				{
					memcpy(responseMessageContent, node, sizeof_node);
					responseMessageContent += sizeof_node;
				}

			}

		}

		// Characters request



		// Send subsequent zmq_send (needed due to ZMQ_REP type socket)
		DOL(doLog, Log, "[DIST Thread] Send message length: %d", responseLength);
		zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
		try {
			socket->send(responseMessage);
		}
		catch (const zmq::error_t &e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[DIST Thread] send exception: %s", *errName);
			return;
		}
		// In case of infinite while
		Sleep(10);


	}
}