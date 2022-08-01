// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "VPETModule.h"

using namespace VPET;

// Sets default values
AVPETModule::AVPETModule()
{
	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

	// Default values
	HostIP = FString("127.0.0.1");
	UseTexture = false;
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

// Called when the game starts or when spawned
void AVPETModule::BeginPlay()
{
	Super::BeginPlay();

	// Host IP identifier
	int32 idIndex = INDEX_NONE;
	if (HostIP.FindLastChar('.', idIndex))
	{
		FString onlyID = HostIP.RightChop(idIndex + 1);
		m_id = FCString::Atoi(*onlyID);
	}
	DOL(LogBasic, Log, "[VPET2 BeginPlay] M_ID: %d", m_id);
	OSD(FColor::Cyan, "[VPET2 BeginPlay] M_ID: %d", m_id);
	
	DOL(LogBasic, Warning, "[VPET2 BeginPlay] Game began.");

	// Grab world
	UWorld* MyWorld = GetWorld();
	// And level if needed
	ULevel* CurrLevel = MyWorld->GetCurrentLevel();
	// Print variable
	DOL(LogBasic, Log, "[VPET2 BeginPlay] World name: %s", *MyWorld->GetName());

	// Start populating the distributor state

	// Header values
	m_state.vpetHeader.lightIntensityFactor = 1.0;
	//m_state.vpetHeader.textureBinaryType = 0;

	// set LOD / tagging mode (from Katana)
	m_state.lodMode = TAG;
	m_state.lodTag = "lo";

	DOL(LogBasic, Log, "[VPET BeginPlay] Building scene...");

	// World tweak - creating a global root
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
	}

	// Edit root child count
	m_state.nodeList.at(0)->childCount = rootChildrenCount;
	DOL(LogBasic, Log, "[VPET BeginPlay] Root children count: %i", m_state.nodeList.at(0)->childCount);

	// Print stats
	DOL(LogBasic, Log, "[VPET BeginPlay] Texture Count: %i", m_state.texPackList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Material Count: %i", m_state.matPackList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Object(Mesh) Count: %i", m_state.objPackList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Node Count: %i", m_state.nodeList.size());
	DOL(LogBasic, Log, "[VPET BeginPlay] Objects: %i", m_state.numObjectNodes);
	DOL(LogBasic, Log, "[VPET BeginPlay] Lights: %i", m_state.numLights);
	DOL(LogBasic, Log, "[VPET BeginPlay] Cameras: %i", m_state.numCameras);


	// Open ØMQ context
	context = new zmq::context_t(1);


	// Distribution thread - for hosting the scene - TODO promote the port value to somewhere easier to find
	FString distributionPort(":5555");
	socket_d = new zmq::socket_t(*context, ZMQ_REP);

	// Safe attempt to bind - if socket exists, stop it all
	try {
		FString hostAddress = FString("tcp://") + HostIP + distributionPort;
		std::string hostString(TCHAR_TO_UTF8(*hostAddress));
		socket_d->bind(hostString);
	}
	catch (const zmq::error_t& e)
	{
		FString errName = FString(zmq_strerror(e.num()));
		DOL(LogBasic, Error, "[VPET2 BeginPlay] ERROR Distribution - Failed to bind: %s", *errName);
		OSD(FColor::Red, "[VPET2 BeginPlay] ERROR Distribution - Failed to bind: %s", *errName);
		return;
	}

	DOL(LogBasic, Warning, "[VPET2 BeginPlay] Distribution socket created!");
	OSD(FColor::Cyan, "[VPET2 BeginPlay] Distribution socket created!");

	// Start distribution (request / reply) thread
	auto tDistribute = new FAutoDeleteAsyncTask<SceneSenderThread>(socket_d, &m_state, LogBasic);
	tDistribute->StartBackgroundTask();


	// Update Receiver Thread
	FString updateReceiverPort(":5556");
	// Prepare ZMQ Subscriber socket
	socket_r = new zmq::socket_t(*context, ZMQ_SUB);
	// cpp method
	socket_r->setsockopt(ZMQ_SUBSCRIBE, "", 0);

	// Safe attempt to connect - if socket exists, stop it all
	try {
		FString hostAddress = FString("tcp://") + HostIP + updateReceiverPort;
		std::string hostString = TCHAR_TO_UTF8(*hostAddress);
		socket_r->connect(hostString);
	}
	catch (const zmq::error_t& e)
	{
		FString errName = FString(zmq_strerror(e.num()));
		DOL(LogBasic, Error, "[VPET2 BeginPlay] ERROR Receiver - Failed to connect: %s", *errName);
		OSD(FColor::Red, "[VPET2 BeginPlay] ERROR Receiver - Failed to connect: %s", *errName);
		return;
	}

	DOL(LogBasic, Warning, "[VPET2 BeginPlay] Update receiver socket created!");
	OSD(FColor::Cyan, "[VPET2 BeginPlay] Update receiver socket created!");

	// Start the message queues
	msgQ.clear();
	msgQs.clear();
	msgData.clear();
	msgLen.clear();

	// Start synchronization (receiver) thread
	auto tUpdateReceiver = new FAutoDeleteAsyncTask<UpdateReceiverThread>(socket_r, &msgQ, m_id, LogBasic, this);
	tUpdateReceiver->StartBackgroundTask();


	// Update Sender Thread
	FString updateSenderPort(":5557");
	// Prepare ZMQ Publisher socket
	socket_s = new zmq::socket_t(*context, ZMQ_PUB);

	// Safe attempt to connect - if socket exists, stop it all
	try {
		FString hostAddress = FString("tcp://") + HostIP + updateSenderPort;
		std::string hostString = TCHAR_TO_UTF8(*hostAddress);
		socket_s->connect(hostString);
	}
	catch (const zmq::error_t& e)
	{
		FString errName = FString(zmq_strerror(e.num()));
		DOL(LogBasic, Error, "[VPET2 BeginPlay] ERROR Sender - Failed to connect: %s", *errName);
		OSD(FColor::Red, "[VPET2 BeginPlay] ERROR Sender - Failed to connect: %s", *errName);
		return;
	}

	DOL(LogBasic, Warning, "[VPET2 BeginPlay] Update sender socket created!");
	OSD(FColor::Cyan, "[VPET2 BeginPlay] Update sender socket created!");

	// Start synchronization (sender) thread
	auto tUpdateSender = new FAutoDeleteAsyncTask<UpdateSenderThread>(socket_s, &msgQs, m_id, LogBasic, &msgData, &msgLen);//, &runThreadSend);
	tUpdateSender->StartBackgroundTask();


	// Manage editor selection changes 
	FLevelEditorModule& levelEditor = FModuleManager::GetModuleChecked<FLevelEditorModule>("LevelEditor");
	FLevelEditorModule::FActorSelectionChangedEvent fasce = levelEditor.OnActorSelectionChanged();
	levelEditor.OnActorSelectionChanged().AddUObject(this, &AVPETModule::HandleOnActorSelectionChanged);
}


// Called every frame
void AVPETModule::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	// Process messages
	int count = 0;
	for (size_t i = 0; i < msgQ.size(); i++)
	{
		std::vector<uint8_t> msg = msgQ.at(i);
		ParseParameterUpdate(msg);
		count++;
	}

	// Clean processed messages
	msgQ.erase(msgQ.begin(), msgQ.begin() + count);


	// Development prints
	if (doItOnce)
	{
		doItOnce = false;
		DOL(LogBasic, Log, "[VPET Once] Editable actors list:");
		for (size_t i = 0; i < actorList.Num(); i++)
		{
			// Grab back the actors
			AActor* aActor = actorList[i];
			if (aActor)
			{
				FString aName = aActor->GetActorLabel();
				DOL(LogBasic, Log, "[VPET Once] %d: %s", i, *aName);
			}
		}

		// checking instancing
		int objCount = m_state.objPackList.size();
		DOL(LogBasic, Log, "[VPET Once] Unique object count: %d", objCount);
	}

}


void AVPETModule::ParseParameterUpdate(std::vector<uint8_t> kMsg)
{
	// Development print - print a bunch of bytes
	// Grab a byte
	//uint8_t fByte;
	//for (size_t i = 0; i < 14; i++)
	//{
	//	fByte = kMsg[i];
	//	DOL(LogBasic, Log, "[Parse] Byte: %d, value: %d", i, fByte);
	//}

	int16_t objectID = *reinterpret_cast<int16_t*>(&kMsg[3]);
	DOL(LogBasic, Log, "[SYNC Parse] obj Id: %d", objectID);
	//OSD(FColor::Yellow, "[SYNC Parse] obj Id: %d", objectID);

	if (actorList.Num() <= objectID)
	{
		DOL(LogBasic, Error, "[SYNC Parse] Failed to grab object refered by Id: %d", objectID);
		OSD(FColor::Red, "[SYNC Parse] Failed to grab object refered by Id: %d", objectID);
		return;
	}

	AActor* sceneActor = actorList[objectID];

	int16_t paramID = *reinterpret_cast<int16_t*>(&kMsg[5]);
	DOL(LogBasic, Log, "[SYNC Parse] Param ID: %d", paramID);

	//ParameterType paramType = (ParameterType)kMsg[7];
	//DOL(LogBasic, Log, "[SYNC Parse] Param type: %d", paramType);

	// grab scene object and its parameters
	USceneObject* sceneObj = objectList[objectID];
	TArray<AbstractParameter*>* tempArray = sceneObj->GetParameterList();

	if (paramID < tempArray->Num())
	{
		// pass actual message to a parameter from an object
		AbstractParameter* tempParam = (*tempArray)[paramID];
		tempParam->ParseMessage(kMsg);
	}
	else
		DOL(LogBasic, Error, "[SYNC Parse] Trying to edid param ID %d but it's not available", paramID);


}


// Called when the game ends
void AVPETModule::EndPlay(const EEndPlayReason::Type EndPlayReason)
{
	Super::EndPlay(EndPlayReason);

	DOL(LogBasic, Warning, "[VPET2 EndPlay] Game ended.");

	// Stop distribution thread
	DOL(LogBasic, Warning, "[VPET2 Endplay] Closing Zmq distribution socket...");
	if (socket_d)
		socket_d->close();
	delete socket_d;

	// Stop listener thread
	DOL(LogBasic, Warning, "[VPET2 Endplay] Closing Zmq update receiver socket...");
	if (socket_r)
		socket_r->close();
	delete socket_r;

	// Stop sender thread
	DOL(LogBasic, Warning, "[VPET2 Endplay] Closing Zmq update sender socket...");
	if (socket_s)
		socket_s->close();
	delete socket_s;

	DOL(LogBasic, Warning, "[VPET2 Endplay] Destroying Zmq context...");
	if (context)
		context->close();
	delete context;

}

void AVPETModule::HandleOnActorSelectionChanged(const TArray<UObject*>& NewSelection, bool bForceRefresh)
{
	UE_LOG(LogTemp, Log, TEXT("Selection change"));
	int objI = 0;
	std::vector<uint8_t> byteVector;
	
	TArray<UObject*> newSelectList;

	for (UObject* obj : NewSelection)
	{
		byteVector.clear();
		UE_LOG(LogTemp, Log, TEXT("Selection: %s"), *obj->GetName());

		newSelectList.Add(obj);

		// grab object id
		actorList.Find((AActor*)obj, objI);


		// Send lock
		if (objI != INDEX_NONE && !objectList[objI]->_lock)
			EncodeLockMessage(objI, true);
	}
	

	// deselect
	for (UObject* obj : selectedList)
	{

		if (newSelectList.Find(obj) == INDEX_NONE)
		{
			// Deselect
			actorList.Find((AActor*)obj, objI);

			// Make the unlock command
			if (objI != INDEX_NONE && !objectList[objI]->_lock)
				EncodeLockMessage(objI, false);
		}
		
	}
	// refresh list
	selectedList = newSelectList;
	
	


	return;
}


// Scene root construction
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

	// Null entry needed for proper indexing
	AddActorPointer(NULL);
	objectList.Add(NULL);
}

bool AVPETModule::buildLocation(AActor* prim)
{
	// TODO
	// check LOD or equivalent tag

	// VPET2 Hack - object type for enum
	ObjectType objType = ObjectType::NODE;

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

	// Skip spawn camera
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

	if (className == "StaticMeshActor" || prim->ActorHasTag(TEXT("mesh"))) {
		m_state.node = new NodeGeo();
		buildNode((NodeGeo*)m_state.node, prim);
		DOL(LogBasic, Log, "[DIST buildLocation] Found geo actor, creating geo node out of %s; class: %s", *aName, *className);
		objType = ObjectType::GEO;
	}
	// Alternative to strict option: else if (className == "CameraActor") - this includes CineCameraActor
	else if (className.Find("CameraActor") > -1) {
		m_state.node = new NodeCam();
		buildNode((NodeCam*)m_state.node, prim);
		DOL(LogBasic, Log, "[DIST buildLocation] Found camera actor, creating camera node out of %s; class: %s", *aName, *className);
		tweakRot = true;
		objType = ObjectType::CAMERA;
	}
	else if (className.Find("Light") > -1) {
		m_state.node = new NodeLight();
		buildNode((NodeLight*)m_state.node, prim, className);
		DOL(LogBasic, Log, "[DIST buildLocation] Found light actor, creating light node out of %s; class: %s", *aName, *className);
		tweakRot = true;
		if (className.Find("Directional") > -1)
			objType = ObjectType::DIRECTIONALLIGHT;
		else if (className.Find("Spot") > -1)
			objType = ObjectType::SPOTLIGHT;
		else if (className.Find("Point") > -1)
			objType = ObjectType::POINTLIGHT;
		else if (className.Find("Rect") > -1)
			objType = ObjectType::AREALIGHT;
		else
			objType = ObjectType::LIGHT;
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

	// use relative transform if has parent
	if (prim->GetAttachParentActor())
		aTrans = UKismetMathLibrary::MakeRelativeTransform(aTrans, prim->GetAttachParentActor()->GetActorTransform());

	// Decompose
	FVector aPos = aTrans.GetTranslation();
	FQuat aRot = aTrans.GetRotation();
	FVector aSca = aTrans.GetScale3D();

	// Scale position values
	aPos *= .01;

	// Tweak rotation if it is light (due to axis conventions)
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

			// Add the scene object component
			// Note: code formerly used was as follows:
			// prim->AddComponentByClass(USceneObject::StaticClass(), false, emptyTransform, false);
			// USceneObject* obj = Cast<USceneObject>(prim->GetComponentByClass((USceneObject::StaticClass())));
			// Problem: using AddComponentByClass worked for having it registered automatically, but as soon as
			// any parameter is adjusted via the Details panel, the component is unregistered (and ceases to 
			// operate). Via NewObject, the component is unregistered, but instantly re-registered (effects can 
			// be monitored with virtual fuctions OnRegister / OnUnregister

			// Scene object that holds the parameter object to be added
			USceneObject* obj;

			// Add the specific node components
			switch (objType)
			{
			case AVPETModule::ObjectType::AREALIGHT:
				obj = NewObject<USceneObjectAreaLight>(prim, USceneObjectAreaLight::StaticClass());
				break;
			case AVPETModule::ObjectType::POINTLIGHT:
				obj = NewObject<USceneObjectPointLight>(prim, USceneObjectPointLight::StaticClass());
				break;
			case AVPETModule::ObjectType::SPOTLIGHT:
				obj = NewObject<USceneObjectSpotLight>(prim, USceneObjectSpotLight::StaticClass());
				break;
			case AVPETModule::ObjectType::DIRECTIONALLIGHT:
				obj = NewObject<USceneObjectDirectionalLight>(prim, USceneObjectDirectionalLight::StaticClass());
				break;
			case AVPETModule::ObjectType::LIGHT:
				obj = NewObject<USceneObjectLight>(prim, USceneObjectLight::StaticClass());
				break;
			case AVPETModule::ObjectType::CAMERA:
				obj = NewObject<USceneObjectCamera>(prim, USceneObjectCamera::StaticClass());
				break;
			default:
				obj = NewObject<USceneObject>(prim, USceneObject::StaticClass());
				break;
			}

			if (!obj)
				return NULL;
			obj->RegisterComponent();
			obj->SetID(objectList.Num());
			obj->SetcID(m_id);
			objectList.Add(obj);
			obj->SetSenderQueue(&msgData, &msgLen);

			// Warn in case is not movable
			if (!prim->IsRootComponentMovable())
			{
				DOL(LogBasic, Warning, "[DIST buildLocation] Actor %s is set to editable but is not movable! This might lead to issues.", *nName);
				OSD(FColor::Orange, "[DIST buildLocation] Actor %s is set to editable but is not movable! This might lead to issues.", *nName);
			}
		}
	}
	// Not using tag for object selection
	else
	{
		// as long as movable, set to editable
		m_state.node->editable = prim->IsRootComponentMovable();
		AddActorPointer(prim);

		// Scene object that holds the parameter object to be added
		USceneObject* obj;

		switch (objType)
		{
		case AVPETModule::ObjectType::LIGHT:
			obj = NewObject<USceneObjectLight>(prim, USceneObjectLight::StaticClass());
			break;
		case AVPETModule::ObjectType::CAMERA:
			obj = NewObject<USceneObjectCamera>(prim, USceneObjectCamera::StaticClass());
			break;
		default:
			obj = NewObject<USceneObject>(prim, USceneObject::StaticClass());
			break;
		}

		if (!obj)
			return NULL;
		obj->RegisterComponent();
		obj->SetID(objectList.Num());
		obj->SetcID(m_id);
		objectList.Add(obj);
		obj->SetSenderQueue(&msgData, &msgLen);
	}

	// Development counter hack - add sub group for counter-rotating children, if it has any
	if (tweakRot && attActors.Num() > 0)
	{
		m_state.nodeList.at(nodeIndex)->childCount = 1;
		buildEmptyRotator(nName);
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

	//// development check
	//DOL(LogBasic, Warning, "[DIST buildLocation] Local node %s has %d children", *nName, childCount);

	//FString lName(m_state.nodeList.at(nodeIndex)->name);
	//DOL(LogBasic, Warning, "[DIST buildLocation] List node %s has %d children", *lName, m_state.nodeList.at(nodeIndex)->childCount);

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


void AVPETModule::buildNode(NodeGeo* node, AActor* prim)
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

	// Extend options
	DOL(LogGeoBuild, Warning, "[DIST buildNode] Num of meshes: %d", staticMeshComponents.Num());
	DOL(LogGeoBuild, Warning, "[DIST buildNode] Vertex Count 1 : %d", staticMesh->GetNumVertices(0));
	DOL(LogGeoBuild, Warning, "[DIST buildNode] Num LODs: %d", staticMesh->GetNumLODs());
	DOL(LogGeoBuild, Warning, "[DIST buildNode] Min LOD: %d", staticMesh->MinLOD.Default);
	DOL(LogGeoBuild, Warning, "[DIST buildNode] Num LOD resources: %d", staticMesh->RenderData->LODResources.Num());

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

		TArray<FVector> vertices = TArray<FVector>();

		// Validity checks?
		if (!prim->IsValidLowLevel()) return;
		if (!staticMeshComponent) return;;
		if (!staticMesh) return;
		if (!staticMesh->RenderData) return;


		if (staticMesh->RenderData->LODResources.Num() > 0)
		{
			// Before - use the first -> high LOD?
			//FStaticMeshLODResources &resource = staticMesh->RenderData->LODResources[0];
			// Modified - use last -> low LOD?
			FStaticMeshLODResources& resource = staticMesh->RenderData->LODResources[staticMesh->RenderData->LODResources.Num() - 1];

			// Index Buffer
			FRawStaticIndexBuffer& ib = resource.IndexBuffer;
			// TODO - does it need to check if(&ib)?

			// Build indices list 
			FIndexArrayView arrV = ib.GetArrayView();
			for (size_t j = arrV.Num(); j > 0; j--) {
				objPack.indices.push_back(arrV[j - 1]);
			}

			// Vertex Buffer
			FStaticMeshVertexBuffers& vbs = resource.VertexBuffers;
			// Color VertexBuffer
			//FColorVertexBuffer &colVP = vbs.ColorVertexBuffer;
			//for (size_t j = 0; j < colVP.GetNumVertices(); j++) {
			//	DOL(LogBasic, Log, "i: %d - VCol: %f, %f, %f", j, colVP.VertexColor(j).R, colVP.VertexColor(j).G, colVP.VertexColor(j).B);
			//}
			// Position VertexBuffer
			FPositionVertexBuffer& posVP = vbs.PositionVertexBuffer;
			for (size_t j = 0; j < posVP.GetNumVertices(); j++) {
				//DOL(LogBasic, Log, "i: %d - VPos: %f, %f, %f", j, posVP.VertexPosition(j).X, posVP.VertexPosition(j).Y, posVP.VertexPosition(j).Z);
				// Push to pack
				objPack.vertices.push_back(-posVP.VertexPosition(j).X * 0.01);
				objPack.vertices.push_back(posVP.VertexPosition(j).Z * 0.01);
				objPack.vertices.push_back(posVP.VertexPosition(j).Y * 0.01);
			}

			// Mesh vertex buffer
			FStaticMeshVertexBuffer& smVP = vbs.StaticMeshVertexBuffer;
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
	UMaterialInterface* cMat = staticMeshComponent->GetMaterial(0);

	DOL(LogMaterial, Warning, "[DIST buildNode] num mat: %d", staticMeshComponent->GetNumMaterials());
	if (cMat)
		DOL(LogMaterial, Log, "[DIST buildNode] cMat: %s", *cMat->GetName());

	// Development tests for understanding texture assignment
	// Not essential for execution
	if (true)
	{
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
		}

		// all textures?
		TArray<FMaterialTextureInfo> texStr = cMat->GetTextureStreamingData();
		for (size_t j = 0; j < texStr.Num(); j++)
		{
			FMaterialTextureInfo kTex = texStr[j];
			DOL(LogMaterial, Warning, "streaming data texture %d named: %s", j, *kTex.TextureName.ToString());
		}
	}

	// Development tests for understanding material composition
	// If material is made by a Vec3 parameter connected to Base Color, this will find the color values and attribute it to the node
	if (true)
	{
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
	}

	if (cMat)
	{
		// VPET2 Material test
		FString matName = *cMat->GetName();

		// Create material package
		MaterialPackage matPack;

		bool prepTexture = false;
		// Name
		matPack.name = TCHAR_TO_ANSI(*matName);

		// try to populate

		// type
		matPack.type = 0;
		// src
		matPack.src = TCHAR_TO_ANSI(*FString("Standard"));

		// shader config // 9 shaderKeywords
		for (size_t j = 0; j < 9; j++)
			matPack.shaderConfig.push_back(false);

		// properties

		// first one being color
		matPack.shaderPropertyIds.push_back(0);
		
		// type - color or texture
		matPack.shaderPropertyTypes.push_back(0);
		if (UseTexture)
			matPack.shaderPropertyTypes.push_back(4);

		// case color RGBA
		float f;
		unsigned char const* p;
		// R
		f = node->color[0];
		p = reinterpret_cast<unsigned char const*>(&f);
		matPack.shaderProperties.push_back(p[0]);
		matPack.shaderProperties.push_back(p[1]);
		matPack.shaderProperties.push_back(p[2]);
		matPack.shaderProperties.push_back(p[3]);
		// G
		f = node->color[1];
		p = reinterpret_cast<unsigned char const*>(&f);
		matPack.shaderProperties.push_back(p[0]);
		matPack.shaderProperties.push_back(p[1]);
		matPack.shaderProperties.push_back(p[2]);
		matPack.shaderProperties.push_back(p[3]);
		// B
		f = node->color[2];
		p = reinterpret_cast<unsigned char const*>(&f);
		matPack.shaderProperties.push_back(p[0]);
		matPack.shaderProperties.push_back(p[1]);
		matPack.shaderProperties.push_back(p[2]);
		matPack.shaderProperties.push_back(p[3]);
		// A
		f = 1.0f;
		p = reinterpret_cast<unsigned char const*>(&f);
		matPack.shaderProperties.push_back(p[0]);
		matPack.shaderProperties.push_back(p[1]);
		matPack.shaderProperties.push_back(p[2]);
		matPack.shaderProperties.push_back(p[3]);


		int propertyCount = 26;
		// populate the rest with nothing
		for (size_t j = 0; j < propertyCount; j++)
		{
			matPack.shaderPropertyIds.push_back(0);
			matPack.shaderPropertyTypes.push_back(-1);
			//matPack.shaderProperties.push_back(0);
		}

		if (UseTexture)
		{
			// pop back one item - because of the texture added before (a bit of a dirty trick)
			matPack.shaderPropertyTypes.pop_back();

			// force material to texture?
			for (size_t j = 0; j < 2; j++)
				matPack.textureOffsets.push_back(0);
			for (size_t j = 0; j < 2; j++)
				matPack.textureScales.push_back(1);

			// identify texture index
			int texInd;
			if (matNameList.Find(matName, texInd))
			{
				DOL(LogMaterial, Log, "Found texture for %s, IDX %d", *matName, texInd);
			}
			else
			{
				texInd = matNameList.Num();
				DOL(LogMaterial, Log, "Making texture for %s, IDX %d", *matName, texInd);
				matNameList.Add(matName);
				prepTexture = true;
			}
			matPack.textureIds.push_back(texInd);
			DOL(LogMaterial, Log, "Pushed texture for for %s, IDX %d", *matName, texInd);
		}

		// store the material package
		m_state.matPackList.push_back(matPack);

		// set material id
		node->materialId = m_state.matPackList.size() - 1;

		// prepare texture if needed
		if (prepTexture)
		{
			DOL(LogMaterial, Log, "Preparing texture for %s", *matName);
			TexturePackage texPack;

			// Standard size
			texPack.width = 256;
			texPack.height = 256;
			texPack.colorMapDataSize = 0;

			// Attempt to loading external file
			IPlatformFile& PlatformFile = IPlatformFile::GetPlatformPhysical();
			// Get location of created pre-encoded texture files
			FString texLocation = FPaths::GameSourceDir() + TEXT("../Intermediate/VPET/");
			// Prefix of texture file names
			FString texPrefix = TEXT("VPETtex_");
			// Build file path
			FString filename = texLocation + texPrefix + matName + FString(".astc");
			IFileHandle* File = PlatformFile.OpenRead(*filename);

			if (File)
			{
				int fileSize = File->Size();
				DOL(LogMaterial, Log, "Texture file open for %s, size %d", *matName, fileSize);
				texPack.colorMapDataSize = fileSize;
				uint8* rawData;
				rawData = (uint8*)malloc(fileSize);
				File->Read(rawData, fileSize);
				unsigned char* texData;
				texData = (unsigned char*)malloc(texPack.colorMapDataSize);
				for (size_t j = 0; j < texPack.colorMapDataSize; j++)
					texData[j] = rawData[j + 16];

				texPack.colorMapData = texData;
			}
			else
				DOL(LogBasic, Error, "Couldn't open texture file for %s. Make sure textures were set-up through the VPETWindow plugin.", *matName);

			m_state.texPackList.push_back(texPack);

			// Close file
			delete File;
		}

	}

	// store at sharedState to access it in iterator
	m_state.node = node;
	m_state.numObjectNodes++;
}

void AVPETModule::buildNode(NodeCam* node, AActor* prim)
{
	m_state.node = node;
	m_state.nodeTypeList.push_back(NodeType::CAMERA);

	// Grab camera
	ACameraActor* kCam = Cast<ACameraActor>(prim);
	UCameraComponent* kCamComp = kCam->GetCameraComponent();

	float aspect = kCamComp->AspectRatio;

	// Convert horizontal fov to vertical
	float fov = 2 * atan(tan(kCamComp->FieldOfView / 2.0 * DEG2RAD) / aspect) / DEG2RAD;

	node->fov = fov; // Note: CineCameras have no exposed FoV property but it's focal length correlates to a FoV automatically

	node->aspect = aspect;

	// magic number tests
	node->nearPlane = 0.001;
	node->farPlane = 100;;
	// Always make editable?
	//node->editable = true;

	// store at sharedState to access it in iterator
	m_state.node = node;
	m_state.numCameras++;
}

void AVPETModule::buildNode(NodeLight* node, AActor* prim, FString className)
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
	else if (className.Find("Rect") > -1) {
		node->type = VPET::AREA;

		ARectLight* kRectLgt = Cast<ARectLight >(kLgt);
		if (kRectLgt)
		{
			URectLightComponent* rectLgtCmp = kRectLgt->RectLightComponent;

			// Width can be found at rectLgtCmp->SourceWidth;
			// Height can be found at rectLgtCmp->SourceHeight;
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

	// store at sharedState to access it in iterator
	m_state.node = node;
	m_state.numLights++;

}

void AVPETModule::EncodeLockMessage(int16_t objID, bool lockState)
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 6;
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// header
	uint8_t intVal = m_id;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// time
	intVal = 23; //m_id
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type lock
	intVal = 1;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// object id - int 16
	int16_t shortVal = objID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// true?
	bool lockVal = lockState;
	memcpy(responseMessageContent, (char*)&lockVal, sizeof(bool));
	responseMessageContent += sizeof(bool);
	
	msgData.push_back(messageStart);
	msgLen.push_back(responseLength);
}

void AVPETModule::DecodeLockMessage(int16_t* objID, bool* lockState)
{
	DOL(LogBasic, Log, "Lock message Id: %d %d", *objID, *lockState);
	USceneObject* sceneObj = objectList[*objID];
	sceneObj->_lock = *lockState;
}