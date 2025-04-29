# Unity Irregular Hex Grid

## Steps

### 1. Generate a Hexagonal Grid of Points

### 2. Create Triangles
- Connect the points to form triangles.

### 3. Merge Triangles Randomly to Form Quads
- Find triangles with a common side and merge them into quads based on a random probability (to allow some triangles to remain unmerged).

### 4. Adjust the Points So the Quads Approximate a Square
- The quads are adjusted as follows:
  - Calculate the average side length: (A + B + C + D) / 4
  - Calculate half the length of a diagonal.
  - From the quad center to each point, move the point outward to better match the diagonal length.
  - Limit the maximum distance a point can move.
- Since expanding quads can shrink adjacent triangles, limit the minimum size of any triangle.

The irregular shapes result from the remaining unmerged triangles at random positions.

![HexGridGen](https://github.com/user-attachments/assets/486eee6a-640c-4e3a-8c08-561adfbb77da)
