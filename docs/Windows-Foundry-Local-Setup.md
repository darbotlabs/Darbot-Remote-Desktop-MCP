# Windows Foundry Local & Phi-4 Integration Guide

## Overview

The Retro RDP Client now supports **Windows Foundry Local** with **Microsoft Phi-4 models** for offline AI assistance. This enables intelligent RDP management without requiring internet connectivity, providing privacy-first AI capabilities on Windows devices.

## Features

### 🧠 Local AI Processing
- **Offline Operation**: No internet required for AI functionality
- **Privacy-First**: All AI processing happens locally on your device
- **Windows Optimized**: Leverages Windows Machine Learning capabilities
- **Phi-4 Models**: Microsoft's efficient small language models

### 🚀 AI Assistant Capabilities
- **RDP Connection Help**: Intelligent guidance for setting up connections
- **Troubleshooting**: Smart diagnosis of connection issues
- **Security Guidance**: Best practices for secure remote desktop access
- **Performance Optimization**: Tips for improving RDP performance
- **Interface Help**: Navigation assistance for the retro-cyber UI

## Setup Instructions

### 1. Model Installation

Place Phi-4 ONNX models in one of these locations:

```
# Application directory
RetroRDP/Models/

# User profile
%LOCALAPPDATA%/RetroRDP/Models/

# Windows AI cache (if available)
%LOCALAPPDATA%/Microsoft/WindowsAI/Cache/

# Hugging Face cache
%USERPROFILE%/.cache/huggingface/transformers/
```

### 2. Supported Model Files

The application automatically detects these Phi-4 model variants:

- `phi-4.onnx` (Full precision)
- `phi-4-mini.onnx` (Compact version)  
- `phi-4-int4.onnx` (Quantized 4-bit)
- `phi-4-fp16.onnx` (Half precision)
- `microsoft-phi-4.onnx` (Official release)

### 3. Model Sources

Download Phi-4 models from:

- **Microsoft**: Official Phi-4 releases
- **Hugging Face**: Community converted models
- **ONNX Model Zoo**: Optimized versions
- **Windows AI Platform**: Pre-optimized models

### 4. System Requirements

- **OS**: Windows 10/11 (64-bit)
- **Memory**: 4GB RAM minimum, 8GB+ recommended
- **Storage**: 2-8GB for model files
- **CPU**: Modern x64 processor with AVX support

## Configuration

### Environment Variables

```bash
# Optional: Specify custom model directory
RETRO_RDP_MODELS_PATH=C:\MyModels\Phi4

# Optional: Enable verbose AI logging
RETRO_RDP_AI_DEBUG=true
```

### Application Settings

The AI service automatically:

1. **Scans** standard locations for Phi-4 models
2. **Loads** the best available model variant
3. **Falls back** to intelligent response mode if no models found
4. **Optimizes** for local hardware capabilities

## AI Service Architecture

### Core Components

```csharp
// Local AI Service Interface
ILocalAIService aiService = new LocalAIService(logger);
await aiService.InitializeAsync();

// Generate responses
string response = await aiService.GenerateResponseAsync(
    prompt: "How do I connect to a Windows server?",
    systemPrompt: "You are an RDP assistant..."
);

// Streaming responses
await foreach (var chunk in aiService.GenerateStreamingResponseAsync(prompt))
{
    Console.Write(chunk);
}
```

### Model Management

- **Automatic Detection**: Scans multiple directories for models
- **Version Selection**: Chooses optimal model based on hardware
- **Memory Management**: Efficient loading and disposal
- **Error Handling**: Graceful fallback when models unavailable

## Usage Examples

### Basic Commands

```
User: "Help me connect to 192.168.1.100"
AI: "I can help you establish an RDP connection to 192.168.1.100! You'll need..."

User: "RDP connection failed"  
AI: "Let me help troubleshoot your RDP issue! Common problems include..."

User: "Security best practices"
AI: "Security is crucial for RDP! Here are key recommendations..."
```

### Advanced Features

- **Context Awareness**: AI remembers conversation context
- **Intent Recognition**: Understands RDP-specific terminology  
- **Error Diagnosis**: Provides specific troubleshooting steps
- **Learning**: Adapts responses based on user preferences

## Performance Optimization

### Model Selection Guidelines

| Model Variant | Memory Usage | Speed | Quality | Use Case |
|---------------|--------------|-------|---------|----------|
| phi-4-int4.onnx | ~2GB | Fast | Good | Low-end hardware |
| phi-4-fp16.onnx | ~4GB | Medium | Better | Balanced performance |
| phi-4.onnx | ~8GB | Slower | Best | High-end systems |

### Hardware Recommendations

- **Entry Level**: 4GB RAM, use int4 models
- **Standard**: 8GB RAM, use fp16 models  
- **High-End**: 16GB+ RAM, use full precision models
- **GPU**: Future versions will support GPU acceleration

## Troubleshooting

### Common Issues

**Model Not Loading**
```
Solution: Check model file location and permissions
Log: "No Phi-4 model found in standard locations"
```

**Out of Memory**
```
Solution: Use smaller model variant (int4 or mini)
Log: "Failed to load model: Insufficient memory"
```

**Slow Performance**
```
Solution: Enable Windows performance mode
Check: CPU usage and available memory
```

### Debug Mode

Enable verbose logging:

```xml
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="8.0.0" />
```

## Security & Privacy

### Data Handling
- **Local Processing**: No data sent to external servers
- **Model Security**: Models loaded with read-only permissions
- **Memory Protection**: Sensitive data cleared after processing
- **Audit Trail**: Optional logging for compliance

### Best Practices
- Keep models updated for security patches
- Use Windows Defender exclusions for model directories
- Monitor resource usage in production environments
- Implement proper access controls for model files

## Future Enhancements

### Planned Features
- **GPU Acceleration**: DirectML and CUDA support
- **Model Quantization**: Runtime optimization
- **Plugin Architecture**: Custom AI extensions
- **Cloud Hybrid**: Optional cloud fallback
- **Voice Integration**: Speech-to-text capabilities

### Windows AI Platform Integration
- **WinML**: Native Windows Machine Learning
- **DirectML**: GPU-accelerated inference
- **Windows Copilot**: Integration with system AI
- **ONNX Runtime**: Optimized model execution

## API Reference

### ILocalAIService Interface

```csharp
public interface ILocalAIService
{
    bool IsInitialized { get; }
    bool IsModelLoaded { get; }
    string? CurrentModelName { get; }
    
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);
    Task<string> GenerateResponseAsync(string prompt, string? systemPrompt = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> GenerateStreamingResponseAsync(string prompt, string? systemPrompt = null, CancellationToken cancellationToken = default);
    void Dispose();
}
```

### Configuration Options

```csharp
public class LocalAIOptions
{
    public string ModelDirectory { get; set; } = "Models";
    public string PreferredModel { get; set; } = "phi-4-fp16.onnx";
    public int MaxTokens { get; set; } = 2048;
    public float Temperature { get; set; } = 0.7f;
    public bool EnableStreaming { get; set; } = true;
}
```

## Support

For issues related to Windows Foundry Local and Phi-4 integration:

1. **Check Model Files**: Verify models are properly downloaded
2. **Review Logs**: Enable debug logging for detailed information  
3. **System Requirements**: Ensure minimum hardware specifications
4. **GitHub Issues**: Report bugs with model information and logs

---

**Version**: 1.0  
**Compatible Models**: Microsoft Phi-4 family  
**Platform**: Windows 10/11 with .NET 8+  
**Last Updated**: Level 2+ Enhancement - Local AI Integration