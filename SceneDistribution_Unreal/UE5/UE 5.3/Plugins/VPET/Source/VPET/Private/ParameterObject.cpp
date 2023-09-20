// Fill out your copyright notice in the Description page of Project Settings.

#include "ParameterObject.h"


// Sets default values for this component's properties
UParameterObject::UParameterObject()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = true;

	// ...
}


// Called when the game starts
void UParameterObject::BeginPlay()
{
	Super::BeginPlay();
	// ...
	
}


// Called every frame
void UParameterObject::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
	

	// ...
}

void UParameterObject::PrintParams()
{
	for (size_t i = 0; i < _parameterList.Num(); i++)
	{
		AbstractParameter* param = _parameterList[i];
		FString fName(param->GetName().c_str());
		UE_LOG(LogTemp, Warning, TEXT("Parameter list: %s"), *fName);
	}


}