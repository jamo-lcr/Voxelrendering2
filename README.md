A simple voxelrenderer, that Renders a heightmap

## Technologies

- OpenTK
- C#

## Installation

   1. Clone the repository:
      ```bash
      git clone https://github.com/jamo-lcr/Voxelrendering2.git
      ```
   
   2.Navigate to the project directory
      ```bash
     cd Voxelrendering2
   ```
  
   3.Add OpenTK
      ```bash
     dotnet add package OpenTK
     dotnet restore
   ```
   4.Build and Run
   ```bash
     dotnet build
     dotnet run
   ```
## Usage
1. Use the methodes Start(dont remove the camera initialisation(you want to see something )) and Update in the Gameclass to write your code for ex. to render a mesh and move the camera.
2. if you want to render the mesh create a new Meshobject with Vertex[] vertices, uint[] indices Vertex has  following variables (Vector3 Position ,Vector3 Normal,public Vector3 Color)
3. to Add a mesh bash```Renderer.activeScene.addMesh(mesh);``` and to remove a Mesh bash```Renderer.activeScene.removeMesh(mesh);```
