// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectPointLight.h"

// Message parsing pre-declarations
void UpdatePointRange(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectPointLight::BeginPlay()
{
	Super::BeginPlay();

	kPointLgt = Cast<APointLight>(kLit);
	if (kPointLgt)
		pointLgtCmp = kPointLgt->PointLightComponent;
	if (pointLgtCmp)
	{
		float range = pointLgtCmp->AttenuationRadius * rangeFactor;
		new Parameter<float>(range, thisActor, "range", &UpdatePointRange, this);
		rangeBuffer = range;
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectPointLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (pointLgtCmp)
	{
		float range = pointLgtCmp->AttenuationRadius * rangeFactor;

		if (range != rangeBuffer)
		{
			UE_LOG(LogTemp, Warning, TEXT("RANGE CHANGE"));
			rangeBuffer = range;
			EncodeParameterRangeMessage();
		}
	}
}

// Prepare a message for range change
void USceneObjectPointLight::EncodeParameterRangeMessage()
{
	// Prepare the byte array
	char* responseMessageContent = NULL;
	char* messageStart = NULL;
	int responseLength = 0;

	// 8 header + float
	responseLength = 8 + sizeof(float);
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
	// Parameter ID - range
	shortVal = 5;
	memcpy(responseMessageContent, (char*)&shortVal, sizeof(uint16_t));
	responseMessageContent += sizeof(uint16_t);
	// Parameter type - 3 for float
	intVal = 3;
	memcpy(responseMessageContent, (char*)&intVal, sizeof(uint8_t));
	responseMessageContent += sizeof(uint8_t);
	// Actual message
	float msgVal = rangeBuffer;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);

	// Push to queue
	msgData->push_back(messageStart);
	msgLen->push_back(responseLength);
}

// Parses a message for range change
void UpdatePointRange(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type RANGE"));
	APointLight* kPointLgt = Cast<APointLight>(actor);
	UPointLightComponent* pointLgtCmp = NULL;
	if (kPointLgt)
		pointLgtCmp = kPointLgt->PointLightComponent;
	if (pointLgtCmp)
	{
		float rangeFactor = 0.005;

		float lK = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type RANGE: %f"), lK);

		pointLgtCmp->AttenuationRadius = lK / rangeFactor;
	}
}
