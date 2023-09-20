// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include <vector>

#include "ARTypes.h"
#include "Parameter.h"
#include "ParameterObject.h"
#include "SceneObject.generated.h"


class AVPETModule;



UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class VPET_API USceneObject : public UParameterObject
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	USceneObject();

	// Is the sceneObject locked?
	bool _lock = false;

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	// This never gets called from the component
	//virtual void EndPlay(const EEndPlayReason::Type EndPlayReason) override;
	// These were used for debugging - gets called after Editor interactions with the object
	//virtual void OnRegister() override;
	//virtual void OnUnregister() override;

	//AVPETModule* manager;
	

	int cID;

	Parameter<FVector>* Position_Vpet_Param;
	Parameter<FQuat>* Rotation_Vpet_Param;
	Parameter<FVector>* Scale_Vpet_Param;


	// Access to send queue
	std::vector<char*>* msgData;
	std::vector<int>* msgLen;

	

public:	
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;
	
	
	AActor* thisActor;

	TArray<AbstractParameter*> modifiedParameterList;


	TArray<AbstractParameter*> GetModifiedParameterList()
	{
		return modifiedParameterList;
	}

	void PrintFunction()
	{
		UE_LOG(LogTemp, Error, TEXT("PRINTINGINGING"));
	}

	void SetcID(int kID)
	{
		cID = kID;
	}

	void SetID(int kID)
	{
		ID = kID;
	}

	int GetID()
	{
		return ID;
	}

	void SetSenderQueue(std::vector<char*>* pData, std::vector<int>* pLen)
	{
		msgData = pData;
		msgLen = pLen;
	}
	
};
