# SmartSupport — Solution Structure

## Project / Folder Tree

```mermaid
graph TD
	SLN["SmartSupport.slnx"] --> API["SmartSupport.Api (.NET 10)"]

	API --> AI["AI/"]
	API --> CTRL["Controllers/"]
	API --> MODELS["Models/"]
	API --> SVC["Services/"]
	API --> PROG["Program.cs"]
	API --> CFG["appsettings.json / Development"]
	API --> PROPS["Properties/launchSettings.json"]

	AI --> ABS["Abstraction/"]
	AI --> PROV["Providers/"]

	ABS --> ILLM["ILLMClient.cs"]
	ABS --> ABSDTO["DTOs/"]
	ABSDTO --> LLMMET["LLMMetrics.cs"]
	ABSDTO --> LLMRES["LLMResponse.cs"]

	PROV --> OLLAMA["Ollama/"]
	OLLAMA --> OCLIENT["OllamaChatClient.cs"]
	OLLAMA --> ODTO["DTOs/"]
	ODTO --> OREQ["OllamaChatRequest.cs"]
	ODTO --> ORES["OllamaChatResponse.cs"]
	ODTO --> OGEN["OllamaGenerationOptions.cs"]
	ODTO --> OOPT["OllamaOptions.cs"]

	CTRL --> AICTRL["AiAssistantController.cs"]

	MODELS --> MAI["AI/"]
	MAI --> CMET["ChatMetrics.cs"]
	MAI --> CREQ["ChatRequest.cs"]
	MAI --> CRES["ChatResponse.cs"]

	SVC --> SAI["AI/"]
	SAI --> IAISVC["IAiService.cs"]
	SAI --> AISVC["AiAssistantService.cs"]
```

## Layered Flow

```mermaid
graph LR
	Client["HTTP Client"] --> Controller["AiAssistantController"]
	Controller --> IService["IAiService"]
	IService --> Service["AiAssistantService"]
	Service --> ILLM["ILLMClient (abstraction)"]
	ILLM --> Ollama["OllamaChatClient (provider)"]
	Ollama --> OllamaAPI[("Ollama LLM API")]

	Controller -. uses .-> Models["Models/AI: ChatRequest / ChatResponse / ChatMetrics"]
	Service -. maps .-> LLMDtos["AI/Abstraction DTOs: LLMResponse / LLMMetrics"]
```

## Summary

- **Single project:** `SmartSupport.Api` targeting .NET 10 (Web API).
- **AI abstraction layer** (`AI/Abstraction`) defines `ILLMClient` and provider-agnostic DTOs.
- **Provider implementation** (`AI/Providers/Ollama`) implements the abstraction for Ollama.
- **Controllers → Services → LLM abstraction → Provider** is the request pipeline.
- **Models/AI** holds API-facing request/response/metrics contracts.
