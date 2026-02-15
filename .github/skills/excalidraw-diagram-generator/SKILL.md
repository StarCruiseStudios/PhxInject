---
name: excalidraw-diagram-generator
description: 'Generate Excalidraw diagrams from natural language descriptions. Use when asked to "create a diagram", "make a flowchart", "visualize a process", "draw a system architecture", "create a mind map", or "generate an Excalidraw file". Supports flowcharts, relationship diagrams, mind maps, architecture diagrams, class diagrams, and sequence diagrams.'
---

# Excalidraw Diagram Generator

Generate Excalidraw-format diagrams from natural language descriptions. Create visual representations of processes, systems, relationships, and ideas.

## When to Use This Skill

Use when users request:
- "Create a diagram showing..."
- "Make a flowchart for..."
- "Visualize the architecture of..."
- "Draw the class structure..."
- "Show the relationship between..."
- "Create a sequence diagram for..."

**Supported diagram types:**
- 📊 **Flowcharts**: Sequential processes, workflows, decision trees
- 🔗 **Relationship Diagrams**: Entity relationships, system components, dependencies
- 🏗️ **Architecture Diagrams**: System design, module interactions, data flow
- 📦 **Class Diagrams**: Object-oriented design, class structures and relationships
- 🔄 **Sequence Diagrams**: Object interactions over time, message flows
- 🧠 **Mind Maps**: Concept hierarchies, brainstorming results

## Step-by-Step Workflow

### Step 1: Understand the Request

Analyze the user's description to determine:
1. **Diagram type** (flowchart, class diagram, architecture, etc.)
2. **Key elements** (entities, steps, concepts, classes)
3. **Relationships** (flow, connections, hierarchy, dependencies)
4. **Complexity** (number of elements)

### Step 2: Choose the Appropriate Diagram Type

| User Intent | Diagram Type | Example Keywords |
|-------------|--------------|------------------|
| Process flow, steps, procedures | **Flowchart** | "workflow", "process", "steps" |
| Connections, dependencies | **Relationship Diagram** | "relationship", "dependencies" |
| System design, components | **Architecture Diagram** | "architecture", "system", "components" |
| OO design, class structures | **Class Diagram** | "class", "inheritance", "OOP" |
| Interaction sequences | **Sequence Diagram** | "sequence", "interaction", "messages" |
| Concept hierarchy | **Mind Map** | "mind map", "concepts", "ideas" |

### Step 3: Extract Structured Information

**For Class Diagrams (C# focus):**
- Classes with names
- Properties with types and access modifiers (public, private, protected, internal)
- Methods with signatures and access modifiers
- Relationships: inheritance (solid line + white triangle), interface implementation (dashed line + white triangle), composition (solid line + filled diamond), aggregation (solid line + white diamond)
- Generic type parameters

**For Sequence Diagrams:**
- Objects/actors (arranged horizontally at top)
- Lifelines (vertical lines from each object)
- Messages (horizontal arrows between lifelines with method names)
- Async vs sync messages

**For Architecture Diagrams:**
- Components/services
- Data stores
- External systems
- Communication paths and protocols

**For Flowcharts:**
- Sequential steps
- Decision points
- Start and end points

### Step 4: Generate the Excalidraw JSON

Create the `.excalidraw` file with appropriate elements:

**Available element types:**
- `rectangle`: Boxes for classes, components, steps
- `ellipse`: Alternative shapes
- `diamond`: Decision points
- `arrow`: Directional connections
- `text`: Labels and annotations

**Key properties:**
- **Position**: `x`, `y` coordinates
- **Size**: `width`, `height`
- **Style**: `strokeColor`, `backgroundColor`, `fillStyle`
- **Font**: `fontFamily: 5` (Excalifont - **required for all text**)
- **Connections**: `points` array for arrows

### Step 5: Format the Output

Structure the complete Excalidraw file:

```json
{
  "type": "excalidraw",
  "version": 2,
  "source": "https://excalidraw.com",
  "elements": [
    // Array of diagram elements
  ],
  "appState": {
    "viewBackgroundColor": "#ffffff",
    "gridSize": 20
  },
  "files": {}
}
```

### Step 6: Save and Provide Instructions

1. Save as `<descriptive-name>.excalidraw`
2. Inform user how to open:
   - Visit https://excalidraw.com
   - Click "Open" or drag-and-drop the file
   - Or use Excalidraw VS Code extension

## Best Practices

### Element Count Guidelines

| Diagram Type | Recommended Count | Maximum |
|--------------|-------------------|---------|
| Class diagram classes | 3-8 | 12 |
| Flowchart steps | 3-10 | 15 |
| Architecture components | 3-8 | 12 |
| Sequence diagram objects | 3-6 | 10 |

### Layout Tips

1. **Spacing**: 
   - Horizontal gap: 200-300px between elements
   - Vertical gap: 100-150px between rows
2. **Colors**: Use consistent color scheme
   - Classes: Light blue (`#a5d8ff`)
   - Interfaces: Light green (`#b2f2bb`)
   - Important elements: Yellow (`#ffd43b`)
3. **Text sizing**: 16-24px for readability
4. **Font**: Always use `fontFamily: 5` (Excalifont)

### C# Class Diagram Conventions

- Use standard UML notation
- Show access modifiers: `+` (public), `-` (private), `#` (protected), `~` (internal)
- Include important properties and methods
- Show generic type parameters: `ClassName<T>`
- Indicate nullable reference types where relevant

## Example: C# Class Diagram

For a request like "Create a class diagram showing a dependency injection container":

```json
{
  "type": "excalidraw",
  "version": 2,
  "source": "https://excalidraw.com",
  "elements": [
    {
      "type": "rectangle",
      "id": "container-class",
      "x": 100,
      "y": 100,
      "width": 250,
      "height": 200,
      "strokeColor": "#1e1e1e",
      "backgroundColor": "#a5d8ff",
      "fillStyle": "solid"
    },
    {
      "type": "text",
      "id": "container-text",
      "x": 120,
      "y": 120,
      "text": "InjectorBuilder\n---\n+Build(): IInjector\n+Register<T>()\n+RegisterSingleton<T>()",
      "fontSize": 16,
      "fontFamily": 5
    }
  ],
  "appState": {
    "viewBackgroundColor": "#ffffff"
  }
}
```

## Validation Checklist

Before delivering:
- [ ] All elements have unique IDs
- [ ] Coordinates prevent overlapping
- [ ] Text is readable (font size 16+)
- [ ] **All text elements use `fontFamily: 5`**
- [ ] Arrows connect logically
- [ ] Colors follow consistent scheme
- [ ] File is valid JSON
- [ ] Element count is reasonable (<20)

## Limitations

- Complex curves are simplified to straight/basic curved lines
- Hand-drawn roughness is set to default (1)
- No embedded images in auto-generation
- Maximum recommended elements: 20 per diagram

## Output Format

Always provide:
1. ✅ Complete `.excalidraw` JSON file
2. 📊 Summary of what was created
3. 📝 Element count
4. 💡 Instructions for opening/editing

**Example summary:**
```
Created: injector-architecture.excalidraw
Type: Class Diagram
Elements: 5 classes, 8 relationships, 1 title
Total: 14 elements

To view:
1. Visit https://excalidraw.com
2. Drag and drop injector-architecture.excalidraw
3. Or use Excalidraw VS Code extension
```
