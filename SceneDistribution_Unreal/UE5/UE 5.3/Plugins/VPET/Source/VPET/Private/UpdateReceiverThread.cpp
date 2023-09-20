// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "UpdateReceiverThread.h"


// Update receiver thread
void UpdateReceiverThread::DoWork()
{
	DOL(doLog, Warning, "[VPET2 RECV Thread] zeroMQ update receiver thread running");

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
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[RECV Thread] recv exception: %s", *errName);
			return;
		}

		const char* msgPointer = static_cast<const char*>(message.data());
		if (msgPointer == NULL) {
			DOL(doLog, Error, "[RECV Thread] Error msgPointer is NULL");
		}
		else
		{
			// Shifting into std::vector
			byteVector.clear();
			byteStream = static_cast<uint8_t*>(message.data()), message.size();
			for (size_t i = 0; i < message.size(); i++)
			{
				byteVector.push_back(byteStream[i]);
			}

			// Process message 
			// Byte zero -> cID
			// Ignore message from host
			if (byteVector[0] != cID)
			{
				// Byte 1 -> time
				// Byte 2 -> Parameter update
				switch ((MessageType)byteVector[2])
				{
				case MessageType::LOCK:
				{
					//decodeLockMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Lock message"));
					int16_t objectID = *reinterpret_cast<int16_t*>(&byteVector[4]);
					bool lockState = *reinterpret_cast<bool*>(&byteVector[6]);
					manager->DecodeLockMessage(&objectID, &lockState);
					break;
				}
				case MessageType::SYNC:
					//if (!core.isServer)
					//	decodeSyncMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Sync message"));
					break;
				case MessageType::UNDOREDOADD:
					//decodeUndoRedoMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Undo/Redo message"));
					break;
				case MessageType::RESETOBJECT:
					//decodeResetMessage(ref input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Reset message"));
					break;
				case MessageType::PARAMETERUPDATE:
					// input[1] is time
					//m_messageBuffer[input[1]].Add(input);
					UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Parameter updated message"));
					msgQ->push_back(byteVector);
					break;
				default:
					break;
				}
			}
			else
			{
				UE_LOG(LogTemp, Log, TEXT("[VPET2 Parse] Message came from host, cID: %d"), cID);
			}
		}
	}
}
