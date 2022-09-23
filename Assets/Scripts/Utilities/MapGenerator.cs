using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Rendering;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static System.Random worldRand;
    public static Point playerSpawnPoint;
    public static Vector2 lastChunkPos;
    public static bool queuedChunkUpdate = false;
    public static bool allChecksCompleted = false;

    public const int MapWidth = 250;
    public const int MapHeight = 250;
    private const int MinimumRoomSize = 12;
    private const int MaximumRoomSize = 20;
    private const int MaxConnectedHallways = 2;
    private const int MinimumRoomSpreadDistance = MaximumRoomSize + 4;

    private const int MinRooms = 32;
    private const int MaxRooms = 48;

    public static int ChunkSize = 24;
    public static int AmountOfLights = 0;

    public int mapWidth;
    public int mapHeight;
    public int amountOfRooms;
    public GameObject mapContainer;

    public GameObject floorTile;
    public GameObject wallTile;

    public GenerationDetails generationDetails;

    public void UpdateActiveChunk(Vector2 position, bool forced = false)
    {
        if (Vector2.Distance(lastChunkPos, position) >= 5f || forced)
        {
            lastChunkPos = position;
            CubeTile[] mapTiles = mapContainer.GetComponentsInChildren<CubeTile>();
            foreach (CubeTile tile in mapTiles)
                Destroy(tile.gameObject);
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        //CreateWorld();

        Vector3 playerSpawn = new Vector3(playerSpawnPoint.X, 2, playerSpawnPoint.Y);
        //Player.player.SetPosition(playerSpawn);
        DontDestroyOnLoad(this);
        //UpdateActiveChunk(Player.player.topDownPos, true);
        CreateWorld(mapWidth, mapHeight);
    }

    public void CreateMap()
    {
        //CreateWorld();

        Vector3 playerSpawn = new Vector3(playerSpawnPoint.X, 2, playerSpawnPoint.Y);
        //Player.player.SetPosition(playerSpawn);
    }

    // Update is called once per frame
    public void Update()
    {
        //if (allChecksCompleted)
            //UpdateActiveChunk(Player.player.topDownPos);
    }

    public void CreateWorld(int mapWidth, int mapHeight)
    {
        worldRand = new System.Random();

        generationDetails = new GenerationDetails(new byte[1] { Tile.Dirt }, new int[1] { 100 });
        GenerationLayer floorLayer = CreateFloorLayer(mapWidth, mapHeight);
        generationDetails = new GenerationDetails(new byte[1] { Tile.Wall }, new int[1] { 100 });
        GenerationLayer lowerWallLayer = new GenerationLayer(WrapExistingLayer(generationDetails, floorLayer), 2);
        GenerationLayer upperWallLayer = new GenerationLayer(WrapExistingLayer(generationDetails, floorLayer), 3);
        GenerationLayer[] worldLayers = new GenerationLayer[3] { floorLayer, lowerWallLayer, upperWallLayer };
        ConvertLayersTo3D(worldLayers);
    }

    public bool CheckForOOB(Point point, bool createWorld = true)
    {
        bool outOfBounds = point.X < 0 || point.X >= MapWidth || point.Y < 0 || point.Y >= MapHeight;
        return outOfBounds;
    }

    public Tile[,] CreateSquareArea(int area, Point roomCenter, Tile[,] tiles, GenerationDetails details, Tile.GenerationID generationID)
    {
        Point roomTopLeft = new Point(roomCenter.X - (area / 2), roomCenter.Y - (area / 2));
        for (int x = 0; x < area; x++)
        {
            for (int y = 0; y < area; y++)
            {
                byte tileType = details.GetRandomTileType();
                Point tilePoint = new Point(roomTopLeft.X + x, roomTopLeft.Y + y);
                if (CheckForOOB(tilePoint))
                    continue;

                tiles[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, generationID);
            }
        }
        return tiles;
    }

    public GenerationLayer CreateFloorLayer(int width, int height)
    {
        Tile[,] floorLayer = new Tile[width, height];
        for (int x = 0; x < width; x++)      //Creates all the tile instances
        {
            for (int y = 0; y < height; y++)
            {
                Point tilePoint = new Point(x, y);
                floorLayer[x, y] = Tile.CreateTile(Tile.Air, tilePoint);
            }
        }

        int amountOfRooms = worldRand.Next(MinRooms, MaxRooms + 1);
        Rectangle[] rooms = new Rectangle[amountOfRooms];
        Point[] roomCenters = new Point[amountOfRooms];
        for (int i = 0; i < amountOfRooms; i++)
        {
            int roomCenterX = worldRand.Next(2 + (MinimumRoomSize / 2), width - (MinimumRoomSize / 2) - 2);
            int roomCenterY = worldRand.Next(2 + (MaximumRoomSize / 2), height - (MaximumRoomSize / 2) - 2);
            roomCenters[i] = new Point(roomCenterX, roomCenterY);
            if (i == 0)
                playerSpawnPoint = roomCenters[i];

            int roomSize = worldRand.Next(MinimumRoomSize, MaximumRoomSize + 1);

            bool stopRoomGeneration = false;
            rooms[i] = new Rectangle(roomCenters[i], new Point(roomSize));
            for (int j = 0; j < i; j++)
            {
                if (i != j && rooms[i].Intersects(rooms[j]))
                {
                    stopRoomGeneration = true;
                    break;       //just abort creation, let's not deal with overlaps for the funnies.
                }

                if (Vector2.Distance(roomCenters[i].ToVector2(), roomCenters[j].ToVector2()) < MinimumRoomSpreadDistance)
                {
                    stopRoomGeneration = true;
                    break;
                }
            }
            if (stopRoomGeneration)
            {
                roomCenters[i] = Point.Zero;
                rooms[i] = new Rectangle(0, 0, 1, 1);
                continue;
            }

            floorLayer = CreateSquareArea(roomSize, roomCenters[i], floorLayer, generationDetails, Tile.GenerationID.Room);
        }


        int[] roomHallways = new int[amountOfRooms];        //The amount of hallways a room has connected to it.
        for (int i = 0; i < amountOfRooms; i++)     //Base hallway generation
        {
            //0 for Horizontal
            //1 for Vertical
            int connectionStyle = worldRand.Next(0, 1 + 1);
            connectionStyle = 0;

            //int hallwayWidth = 3;
            //if (worldRand.Next(0, 1 + 1) == 0)
            //hallwayWidth = 5;

            if (roomCenters[i] == Point.Zero)
                continue;

            if (roomHallways[i] >= MaxConnectedHallways)
                continue;

            if (connectionStyle == 0)
            {
                int otherRoom = FindClosestRoom(roomCenters[i], roomCenters, roomHallways);
                Point closestRoomPoint = roomCenters[otherRoom];
                Point closestRoomDimensions = new Point(rooms[otherRoom].Width, rooms[otherRoom].Height);

                int xDist = roomCenters[i].X - closestRoomPoint.X;
                int direction = 1;
                if (xDist > 0)
                    direction = -1;
                xDist = Math.Abs(xDist);
                xDist -= (int)Math.Ceiling(rooms[i].Width / 2f) - 2;
                if (direction == -1)
                    xDist += 1;

                int hallwayStartX = roomCenters[i].X + ((int)Math.Ceiling(rooms[i].Width / 2f) * direction);
                for (int x = 0; x < xDist; x++)
                {
                    //for (int y = -2 - (hallwayWidth / 2); y < (hallwayWidth / 2) + 2; y++)        //The hallway will connect with the room's center.
                    for (int y = -3; y < 2; y++)
                    {
                        byte tileType = generationDetails.GetRandomTileType();
                        Point tilePoint = new Point(hallwayStartX + (x * direction), roomCenters[i].Y + y);
                        if (CheckForOOB(tilePoint))
                            continue;

                        floorLayer[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, Tile.GenerationID.Hallway);
                    }
                }

                int yDist = roomCenters[i].Y - closestRoomPoint.Y;
                direction = 1;
                if (yDist > 0)
                    direction = -1;
                yDist = Math.Abs(yDist);
                yDist += 1;
                if (direction == -1)
                    yDist += 1;
                //yDist -= closestRoomDimensions.Y / 2;

                for (int y = 0; y < yDist; y++)
                {
                    //for (int x = -(hallwayWidth / 2) - 1; x < (hallwayWidth / 2) + 1; x++)        //The hallway will connect with the room's center.
                    for (int x = -1; x < 2; x++)
                    {
                        byte tileType = generationDetails.GetRandomTileType();
                        Point tilePoint = new Point(closestRoomPoint.X + x, roomCenters[i].Y + (y * direction));
                        if (CheckForOOB(tilePoint))
                            continue;

                        floorLayer[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, Tile.GenerationID.Hallway);
                    }
                }
                roomHallways[i]++;
                roomHallways[otherRoom]++;
            }
            /*else
            {
                int otherRoom = FindClosestRoom(roomCenters[i], roomCenters, roomHallways);
                Point closestRoomPoint = roomCenters[otherRoom];
                Point closestRoomDimensions = new Point(rooms[otherRoom].Width, rooms[otherRoom].Height);

                int yDist = roomCenters[i].Y - closestRoomPoint.Y;
                int direction = 1;
                if (yDist > 0)
                    direction = -1;
                yDist = Math.Abs(yDist);
                yDist -= closestRoomDimensions.Y / 2;

                int hallwayStartY = roomCenters[i].Y + ((rooms[i].Height / 2) * direction);
                for (int y = 0; y < yDist; y++)
                {
                    //for (int x = -(hallwayWidth / 2); x < (hallwayWidth / 2); x++)        //The hallway will connect with the room's center.
                    for (int x = -1; x < 2; x++)
                    {
                        byte tileType = Tile.Tile_WoodenFloor;
                        Point tilePoint = new Point(roomCenters[i].X + x, hallwayStartY + (y * direction));

                        map[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, generationID: Tile.Generation_Hallway);
                    }
                }

                int xDist = roomCenters[i].X - closestRoomPoint.X;
                direction = 1;
                if (xDist > 0)
                    direction = -1;
                xDist = Math.Abs(xDist);
                xDist -= closestRoomDimensions.X / 2;

                for (int x = 0; x < xDist; x++)
                {
                    //for (int y = -2 - (hallwayWidth / 2); y < (hallwayWidth / 2) + 2; y++)        //The hallway will connect with the room's center.
                    for (int y = -3; y < 2; y++)
                    {
                        byte tileType = Tile.Tile_WoodenFloor;
                        Point tilePoint = new Point(roomCenters[i].X + (x * direction), closestRoomPoint.Y + y);
                        if (y == -3)
                            tileType = Tile.Tile_Wall_Top;
                        else if (y == -2)
                            tileType = Tile.Tile_Wall_Bottom;

                        if (tileType != Tile.Tile_WoodenFloor && map[tilePoint.X, tilePoint.Y].tileType != Tile.Air)
                            continue;

                        map[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, generationID: Tile.Generation_Hallway);
                    }
                }
                roomHallways[i]++;
                roomHallways[otherRoom]++;
            }*/
        }

        int amountOfExtraHallways = 0;//worldRand.Next(0, 5 + 1);
        for (int i = 0; i < amountOfExtraHallways; i++)     //Extra hallway generation. These types of hallways will just connect to already existing tiles.
        {
            int chosenRoomIndex = worldRand.Next(0, amountOfRooms);
            Point chosenRoom = roomCenters[chosenRoomIndex];
            if (chosenRoom == Point.Zero)
                continue;

            Point chosenRoomDimensions = new Point(rooms[chosenRoomIndex].Width, rooms[chosenRoomIndex].Height);

            //0 for Horizontal
            //1 for Vertical
            int connectionStyle = worldRand.Next(0, 1 + 1);

            if (connectionStyle == 0)
            {
                int direction = 1;
                bool noHallwayConnections = true;
                Point connectionPoint = Point.Zero;
                for (int j = 0; j < width; i++)
                {
                    direction = -1;
                    int x = chosenRoom.X + ((chosenRoomDimensions.X / 2) * direction);
                    if (x < 0)
                        break;

                    if (floorLayer[x, chosenRoom.Y].tileType != Tile.Air)
                    {
                        noHallwayConnections = false;
                        connectionPoint = new Point(x, chosenRoom.Y);
                        break;
                    }
                }

                if (noHallwayConnections)
                {
                    for (int j = 0; j < width; i++)
                    {
                        direction = 1;
                        int x = chosenRoom.X + ((chosenRoomDimensions.X / 2) * direction);
                        if (x >= width)
                            break;

                        if (floorLayer[x, chosenRoom.Y].tileType != Tile.Air)
                        {
                            noHallwayConnections = false;
                            connectionPoint = new Point(x, chosenRoom.Y);
                            break;
                        }
                    }
                }

                if (noHallwayConnections)
                    continue;

                int xDist = Math.Abs(chosenRoom.X - connectionPoint.X);
                for (int x = 0; x < xDist; x++)
                {
                    for (int y = -3; y < 2; y++)        //2 walls and 3 floor tiles
                    {
                        byte tileType = generationDetails.GetRandomTileType();
                        Point tilePoint = new Point(chosenRoom.X + ((chosenRoomDimensions.X / 2) * direction), chosenRoom.Y + y);
                        if (CheckForOOB(tilePoint))
                            continue;

                        floorLayer[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, Tile.GenerationID.Hallway);
                    }
                }
            }
            else
            {
                int direction = 1;
                bool noHallwayConnections = true;
                Point connectionPoint = Point.Zero;
                for (int j = 0; j < width; i++)
                {
                    direction = -1;
                    int y = chosenRoom.Y + ((chosenRoomDimensions.Y / 2) * direction);
                    if (y < 0)
                        break;

                    if (floorLayer[chosenRoom.X, y].tileType != Tile.Air)
                    {
                        noHallwayConnections = false;
                        connectionPoint = new Point(chosenRoom.X, y);
                        break;
                    }
                }

                if (noHallwayConnections)
                {
                    for (int j = 0; j < width; i++)
                    {
                        direction = 1;
                        int y = chosenRoom.Y + ((chosenRoomDimensions.Y / 2) * direction);
                        if (y >= height)
                            break;

                        if (floorLayer[chosenRoom.X, y].tileType != Tile.Air)
                        {
                            noHallwayConnections = false;
                            connectionPoint = new Point(chosenRoom.X, y);
                            break;
                        }
                    }
                }

                if (noHallwayConnections)
                    continue;

                int yDist = Math.Abs(chosenRoom.Y - connectionPoint.Y);
                for (int x = -1; x < 2; x++)
                {
                    for (int y = 0; y < yDist; y++)        //2 walls and 5 floor tiles
                    {
                        byte tileType = generationDetails.GetRandomTileType();
                        Point tilePoint = new Point(chosenRoom.X + x, chosenRoom.Y + ((chosenRoomDimensions.Y / 2) * direction));
                        if (CheckForOOB(tilePoint))
                            continue;

                        floorLayer[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint, Tile.GenerationID.Hallway);
                    }
                }
            }
        }


        /*int amountOfExtraHallways = worldRand.Next(15, 24 + 1);
        for (int i = 0; i < amountOfExtraHallways; i++)     //Extra hallway generation. These types of hallways will stop generating if it hits an existing tile, which would make interconnecting hallways (probably)
        {
            int chosenRoomIndex = worldRand.Next(0, amountOfRooms);
            int targetRoomIndex = worldRand.Next(0, amountOfRooms);
            Point chosenRoom = roomCenters[chosenRoomIndex];
            Point targetRoom = roomCenters[targetRoomIndex];
            if (chosenRoomIndex == targetRoomIndex || chosenRoom == Point.Zero || targetRoom == Point.Zero)
                continue;

            Point chosenRoomDimensions = new Point(rooms[chosenRoomIndex].Width, rooms[chosenRoomIndex].Height);
            Point targetRoomDimensions = new Point(rooms[targetRoomIndex].Width, rooms[targetRoomIndex].Height);

            while (Math.Abs(chosenRoom.X - targetRoom.X) < MaximumRoomSize || Math.Abs(chosenRoom.Y - targetRoom.Y) < MaximumRoomSize || targetRoom == Point.Zero)
            {
                targetRoomIndex = worldRand.Next(0, amountOfRooms);
                targetRoom = roomCenters[targetRoomIndex];
                targetRoomDimensions = new Point(rooms[targetRoomIndex].Width, rooms[targetRoomIndex].Height);
            }

            //0 for Horizontal
            //1 for Vertical
            int connectionStyle = worldRand.Next(0, 1 + 1);

            int hallwayWidth = 3;
            if (worldRand.Next(0, 1 + 1) == 0)
                hallwayWidth = 5;

            if (connectionStyle == 0)
            {
                int xDist = chosenRoom.X - targetRoom.X;
                int direction = 1;
                if (xDist > 0)
                    direction = -1;
                xDist = Math.Abs(xDist);
                xDist -= chosenRoomDimensions.X + (targetRoomDimensions.X / 2);

                bool stopHallwayGeneration = false;
                int hallwayStartX = chosenRoom.X + (rooms[i].Width * direction);
                for (int x = 0; x < xDist; x++)
                {
                    for (int y = -2 - (hallwayWidth / 2); y < (hallwayWidth / 2) + 2; y++)        //The hallway will connect with the room's center.
                    {
                        if (map[hallwayStartX + (x * direction), chosenRoom.Y + y].tileType != Tile.Air)
                        {
                            stopHallwayGeneration = true;
                            break;
                        }

                        byte tileType = Tile.Tile_WoodenFloor;
                        Point tilePoint = new Point(hallwayStartX + (x * direction), chosenRoom.Y + y);
                        Vector2 tilePoint = tilePoint.ToVector2() * 16f;
                        if (y == -(hallwayWidth / 2) - 2)
                            tileType = Tile.Tile_Wall_Top;
                        else if (y == -(hallwayWidth / 2) - 1)
                            tileType = Tile.Tile_Wall_Bottom;

                        map[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint);
                    }
                    if (stopHallwayGeneration)
                        break;
                }

                if (stopHallwayGeneration)
                    continue;

                int yDist = chosenRoom.Y - targetRoom.Y;
                direction = 1;
                if (yDist > 0)
                    direction = -1;
                yDist = Math.Abs(yDist);
                //yDist -= chosenRoomDimensions.Y + targetRoomDimensions.Y;

                for (int y = 0; y < yDist; y++)
                {
                    for (int x = -(hallwayWidth / 2); x < (hallwayWidth / 2); x++)        //The hallway will connect with the room's center.
                    {
                        if (map[targetRoom.X + x, chosenRoom.Y + (y * direction)].tileType != Tile.Air)
                        {
                            stopHallwayGeneration = true;
                            break;
                        }

                        byte tileType = Tile.Tile_WoodenFloor;
                        Point tilePoint = new Point(targetRoom.X + x, chosenRoom.Y + (y * direction));
                        Vector2 tilePoint = tilePoint.ToVector2() * 16f;

                        map[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint);
                    }
                    if (stopHallwayGeneration)
                        break;
                }
            }
            else
            {
                int yDist = chosenRoom.Y - targetRoom.Y;
                int direction = 1;
                if (yDist > 0)
                    direction = -1;
                yDist = Math.Abs(yDist);
                //yDist -= chosenRoomDimensions.Y + targetRoomDimensions.Y;

                bool stopHallwayGeneration = false;
                int hallwayStartY = chosenRoom.Y + (rooms[i].Height * direction);
                for (int y = 0; y < yDist; y++)
                {
                    for (int x = -(hallwayWidth / 2); x < (hallwayWidth / 2); x++)        //The hallway will connect with the room's center.
                    {
                        if (map[chosenRoom.X + x, hallwayStartY + y * direction].tileType != Tile.Air)
                        {
                            stopHallwayGeneration = true;
                            break;
                        }

                        byte tileType = Tile.Tile_WoodenFloor;
                        Point tilePoint = new Point(chosenRoom.X + x, hallwayStartY + (y * direction));
                        Vector2 tilePoint = tilePoint.ToVector2() * 16f;

                        map[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint);
                    }
                    if (stopHallwayGeneration)
                        break;
                }

                if (stopHallwayGeneration)
                    continue;

                int xDist = chosenRoom.X - targetRoom.X;
                direction = 1;
                if (xDist > 0)
                    direction = -1;
                xDist = Math.Abs(xDist);
                xDist -= chosenRoomDimensions.X + (targetRoomDimensions.X / 2);

                for (int x = 0; x < xDist; x++)
                {
                    for (int y = -2 - (hallwayWidth / 2); y < (hallwayWidth / 2) + 2; y++)        //The hallway will connect with the room's center.
                    {
                        if (map[chosenRoom.X + (x * direction), targetRoom.Y + y].tileType != Tile.Air)
                        {
                            stopHallwayGeneration = true;
                            break;
                        }

                        byte tileType = Tile.Tile_WoodenFloor;
                        Point tilePoint = new Point(chosenRoom.X + (x * direction), targetRoom.Y + y);
                        Vector2 tilePoint = tilePoint.ToVector2() * 16f;
                        if (y == -(hallwayWidth / 2) - 2)
                            tileType = Tile.Tile_Wall_Top;
                        else if (y == -(hallwayWidth / 2) - 1)
                            tileType = Tile.Tile_Wall_Bottom;

                        map[tilePoint.X, tilePoint.Y] = Tile.CreateTile(tileType, tilePoint);
                    }
                    if (stopHallwayGeneration)
                        break;
                }
            }
            //roomHallways[chosenRoomIndex]++;
            //roomHallways[targetRoomIndex]++;
        }*/
        //UpdateActiveChunk(playerSpawnPoint.ToVector2() * 16f);
        GenerationLayer newGenerationLayer = new GenerationLayer
        {
            width = width,
            height = height,
            layerTiles = floorLayer,
            layerLevel = 1
        };
        return newGenerationLayer;
    }

    public Tile[,] WrapExistingLayer(GenerationDetails generationDetails, GenerationLayer generationLayer)
    {
        Tile[,] newLayerTiles = new Tile[generationLayer.width, generationLayer.height];
        for (int x = 0; x < generationLayer.width; x++)
        {
            for (int y = 0; y < generationLayer.height; y++)
            {
                if (TileAroundInPreviousLayer(x, y, generationLayer))
                    newLayerTiles[x, y] = Tile.CreateTile(generationDetails.GetRandomTileType(), x, y, Tile.GenerationID.Undefined);
            }
        }

        return newLayerTiles;
    }

    /// <summary>
    /// Returns the index of the closest room.
    /// </summary>
    /// <returns></returns>
    public static int FindClosestRoom(Point roomCenter, Point[] roomCenters, int[] roomHallwayCounts)
    {
        int closestRoomIndex = 0;

        int closestDistance = MapWidth;
        for (int i = 0; i < roomCenters.Length; i++)
        {
            if (roomCenters[i] == Point.Zero || roomCenters[i] == roomCenter)
                continue;

            //if (Math.Abs(roomCenter.X - roomCenters[i].X) < MaximumRoomSize / 2 || Math.Abs(roomCenter.Y - roomCenters[i].Y) < MaximumRoomSize / 2)
            //continue;

            int roomDist = (int)Vector2.Distance(roomCenters[i].ToVector2(), roomCenters[i].ToVector2());
            int amountOfHallwaysConnected = roomHallwayCounts[i];
            if (amountOfHallwaysConnected == 0 && roomDist < closestDistance)
            {
                closestDistance = roomDist;
                closestRoomIndex = i;
            }
        }

        return closestRoomIndex;
    }

    public static void ScreenshotMap(GenerationLayer generationLayer)
    {
        Texture2D dungeonResultOverviewTexture = new Texture2D(MapWidth, MapHeight);
        Color32[] resultData = new Color32[MapWidth * MapHeight];
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                byte tileType = generationLayer.layerTiles[x, y].tileType;
                Color32 pixelColor = Color.white;
                /*if (tileType == Tile.Air)
                {
                    pixelColor = Color.black;
                }
                else if (tileType == Tile.Tile_WoodenFloor)
                {
                    pixelColor = new Color32(177, 96, 2, 255);
                }
                else if (tileType == Tile.Tile_Wall_Bottom)
                {
                    pixelColor = new Color32(255, 192, 203, 255);
                }
                else if (tileType == Tile.Tile_Wall_Top)
                {
                    pixelColor = new Color32(232, 192, 203, 255);
                }
                else if (tileType == Tile.Tile_Door)
                {
                    pixelColor = Color.cyan;
                }
                else if (tileType == Tile.Tile_RoomTile_1 || tileType == Tile.Tile_RoomTile_2 || tileType == Tile.Tile_RoomTile_3)
                {
                    pixelColor = new Color32(230, 128, 128, 255);
                }
                else
                {
                    pixelColor = new Color32(128, 0, 128, 255);
                }*/

                resultData[x + y * MapHeight] = pixelColor;
            }
        }
        dungeonResultOverviewTexture.SetPixels32(resultData, 0);
        dungeonResultOverviewTexture.Apply();

        byte[] image = dungeonResultOverviewTexture.EncodeToPNG();
        string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/DungeonScreenshots";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllBytes(directory + "/Image" + ".png", image);
    }

    private bool TileAroundInPreviousLayer(int x, int y, GenerationLayer generationLayer)
    {
        Point tilePoint = new Point(x, y);
        return generationLayer.layerTiles[x, y].tileType == Tile.Air && (generationLayer.CheckTileAbove(tilePoint).tileType != Tile.Air || generationLayer.CheckTileUnder(tilePoint).tileType != Tile.Air || generationLayer.CheckTileLeft(tilePoint).tileType != Tile.Air || generationLayer.CheckTileRight(tilePoint).tileType != Tile.Air);
    }

    private bool TileAroundInPreviousLayer(Point tilePoint, GenerationLayer generationLayer)
    {
        return generationLayer.layerTiles[tilePoint.X, tilePoint.Y].tileType == Tile.Air && (generationLayer.CheckTileAbove(tilePoint).tileType != Tile.Air || generationLayer.CheckTileUnder(tilePoint).tileType != Tile.Air || generationLayer.CheckTileLeft(tilePoint).tileType != Tile.Air || generationLayer.CheckTileRight(tilePoint).tileType != Tile.Air);
    }

    private void ConvertLayersTo3D(GenerationLayer[] generationLayers)
    {
        for (int i = 0; i < generationLayers.Length; i++)
        {
            for (int x = 0; x < generationLayers[i].width; x++)
            {
                for (int y = 0; y < generationLayers[i].height; y++)
                {
                    if (generationLayers[i].layerTiles[x, y].tileType != Tile.Air)
                    {
                        GameObject tile = Instantiate(GetTileTypeEquivalent(generationLayers[i].layerTiles[x, y].tileType), mapContainer.transform);
                        tile.transform.position = new Vector3(x, generationLayers[i].layerLevel + 0.5f, y);
                    }
                }
            }
        }
    }

    public GameObject GetTileTypeEquivalent(byte tileType)
    {
        switch (tileType)
        {
            case Tile.Air:
                return null;
            case Tile.Dirt:
                return floorTile;
            case Tile.Wall:
                return wallTile;
            default:
                return null;
        }
    }

    public static bool TypeMatch(int[] types, int type)
    {
        for (int i = 0; i < types.Length; i++)
        {
            if (type == types[i])
                return true;
        }

        return false;
    }

    public static bool TypeMatch(int targetType, int type)
    {
        return targetType == type;
    }
}