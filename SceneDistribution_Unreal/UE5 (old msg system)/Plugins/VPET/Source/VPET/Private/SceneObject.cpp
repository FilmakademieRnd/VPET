// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObject.h"

// Message parsing pre-declarations
void UpdatePosition(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateRotation(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateScale(std::vector<uint8_t> kMsg, AActor* actor);

// Sets default values for this component's properties
USceneObject::USceneObject()
{
	// Set to be ticked every frame.
	PrimaryComponentTick.bCanEverTick = true;
}

// Called when the game starts
void USceneObject::BeginPlay()
{
	Super::BeginPlay();

	thisActor = GetOwner();

	FVector pos = thisActor->GetActorLocation();
	Parameter<FVector>* position = new Parameter<FVector>(pos, thisActor, "position", &UpdatePosition, this);
	posBuffer = pos; // Comment these assignments out if willing to send fresh scene messages every new play
	FQuat rot = thisActor->GetActorRotation().Quaternion();
	Parameter<FQuat>* rotation = new Parameter<FQuat>(rot, thisActor, "rotation", &UpdateRotation, this);
	rotBuffer = rot;
	FVector sca = thisActor->GetActorScale3D();
	Parameter<FVector>* scale = new Parameter<FVector>(sca, thisActor, "scale", &UpdateScale, this);
	scaBuffer = sca;
}

// Using the update loop to check for local parameter changes
void USceneObject::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (_lock)
		return;

	FVector pos = thisActor->GetActorLocation();
	FQuat rot = thisActor->GetActorRotation().Quaternion();
	FVector sca = thisActor->GetActorScale3D();

	if (pos != posBuffer)
	{
		UE_LOG(LogTemp, Warning, TEXT("LOC CHANGE"));
		posBuffer = pos;
		EncodeParameterPosMessage();
	}
	if (rot != rotBuffer)
	{
		UE_LOG(LogTemp, Warning, TEXT("ROT CHANGE"));
		rotBuffer = rot;
		EncodeParameterRotMessage();
	}
	if (sca != scaBuffer)
	{
		UE_LOG(LogTemp, Warning, TEXT("SCA CHANGE"));
		scaBuffer = sca;
		EncodeParameterScaMessage();
	}
}

// Prepare a message for position change
void USceneObject::EncodeParameterPosMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + vector
	responseLength = 8 + 3 * sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - loc
	shortVal = 0;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 5 for vec3
	intVal = 5;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = - 0.01 * posBuffer[0];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = 0.01 * posBuffer[2];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = 0.01 * posBuffer[1];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for rotation change
void USceneObject::EncodeParameterRotMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header +  quat?
	responseLength = 8 + 4 * sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - rot
	shortVal = 1;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 6 for vec4
	intVal = 6;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = -rotBuffer.X;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = rotBuffer.Z;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = rotBuffer.Y;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = rotBuffer.W;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Prepare a message for scale change
void USceneObject::EncodeParameterScaMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + vector
	responseLength = 8 + 3 * sizeof(float);
	messageStart = responseMessageContent = (char*)malloc(responseLength);
	// CID
	uint8_t intVal = cID;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Time
	intVal = 50;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// type Param
	intVal = 0;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Scene Obj ID
	uint16_t shortVal = ID;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter ID - sca
	shortVal = 2;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 5 for vec3
	intVal = 5;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = scaBuffer[0];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = scaBuffer[2];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = scaBuffer[1];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Parses a message for position change
void UpdatePosition(std::vector<uint8_t> kMsg, AActor* actor)
{
	float lX = *reinterpret_cast<float*>(&kMsg[8]);
	float lY = *reinterpret_cast<float*>(&kMsg[12]);
	float lZ = *reinterpret_cast<float*>(&kMsg[16]);
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type POS: %f %f %f"), lX, lY, lZ);

	// Transform actor pos
	FVector aLoc(-lX, lZ, lY);

	aLoc *= 100.0;

	actor->SetActorRelativeLocation(aLoc);

}

// Parses a message for rotation change
void UpdateRotation(std::vector<uint8_t> kMsg, AActor* actor)
{
	float lX = *reinterpret_cast<float*>(&kMsg[8]);
	float lY = *reinterpret_cast<float*>(&kMsg[12]);
	float lZ = *reinterpret_cast<float*>(&kMsg[16]);
	float lW = *reinterpret_cast<float*>(&kMsg[20]);
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type ROT: %f %f %f %f"), lX, lY, lZ, lW);

	// Transform actor rot
	FQuat aRot(-lX, lZ, lY, lW);

	// In the case of cameras and lights and maybe some other stuff that needs tweak (maybe could be transmitted from scene distribution as a bool already - no double chek)
	FString className = actor->GetClass()->GetName();
	if (className.Find("Light") > -1 || className == "CameraActor")
	{
		//DOL(LogBasic, Log, "[SYNC Parse] Tweak rot!");
		FRotator tempRot(0, 90, 0);
		FQuat transRot = tempRot.Quaternion();
		aRot *= transRot;
	}

	actor->SetActorRelativeRotation(aRot);
}

// Parses a message for scale change
void UpdateScale(std::vector<uint8_t> kMsg, AActor* actor)
{
	float lX = *reinterpret_cast<float*>(&kMsg[8]);
	float lY = *reinterpret_cast<float*>(&kMsg[12]);
	float lZ = *reinterpret_cast<float*>(&kMsg[16]);
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SCALE: %f %f %f"), lX, lY, lZ);

	// transform actor sca
	FVector aSca(lX, lZ, lY);

	actor->SetActorRelativeScale3D(aSca);
}

// Old overrides used for debugging
//void USceneObject::EndPlay(const EEndPlayReason::Type EndPlayReason)
//{
//	Super::EndPlay(EndPlayReason);
//
//	//posBuffer;
//	FString aName = thisActor->GetActorLabel();
//	UE_LOG(LogTemp, Error, TEXT("ENDED OBJECT %s"), *aName);
//	//UE_LOG(LogTemp, Error, TEXT("ENDED GAME"));
//}
//
//void USceneObject::OnRegister()
//{
//	Super::OnRegister();
//
//	//FString aName = thisActor->GetActorLabel();
//	//UE_LOG(LogTemp, Error, TEXT("REGISTERING OBJECT %s"), *aName);
//	UE_LOG(LogTemp, Error, TEXT("REGISTERING OBJECT"));
//}
//
//void USceneObject::OnUnregister()
//{
//	Super::OnUnregister();
//
//	FString aName = thisActor->GetActorLabel();
//	UE_LOG(LogTemp, Error, TEXT("UNREGISTERING OBJECT %s"), *aName);
//}
