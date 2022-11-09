using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static System.Random worldRand;
    public static Point playerSpawnPoint;
    public static Vector2 lastChunkPos;
    public static bool queuedChunkUpdate = false;
    public static bool allChecksCompleted = false;

    public int mapWidth = 80;
    public int mapHeight = 80;
    public int minimumAmountOfRooms = 8;
    public int maximumAmountOfRooms = 14;
    public int minimumRoomSize = 12;
    public int maximumRoomSize = 20;
    public int maxHallwaysPerRoom = 2;
    public int minimumRoomSpreadDistance = 24;      //Should be maximumRoomSize + any number
    public int decorationPadding = 20;
    public int roomDecorationPadding = 1;

    public GameObject mapContainer;

    public GameObject floorTile;
    public GameObject wallTile;

    [Tooltip("An array of the decorations that can spawn outside of the walkable portions of the map. Every object in this array must have a MapDetail script attatched to it!")]
    public GameObject[] outerMapDecorations;
    [Tooltip("An array of the decorations that can spawn inside of the 'rooms' of the map. Every object in this array must have a MapDetail script attatched to it!")]
    public GameObject[] roomStructures;

    public GenerationDetails generationDetails;
    public List<Area> roomAreas;
    public GenerationLayer[] worldData;
    public Dictionary<GenerationLayer, bool[,]> occupiedLayerTiles;

    public struct Area      //Pretty much a rectangle but with easier-to-get fields.
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public Point Position { get; }
        public Point Center { get; }
        public Point Dimensions { get; }
        public int Size { get; }

        public Area(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Position = new Point(x, y);
            Dimensions = new Point(width, height);
            Center = Position + (Dimensions / 2);
            Size = width * height;
        }

        public Area(Point position, int width, int height)
        {
            X = position.X;
            Y = position.Y;
            Width = width;
            Height = height;
            Position = position;
            Dimensions = new Point(width, height);
            Center = Position + (Dimensions / 2);
            Size = width * height;
        }

        public Area(Point position, Point dimensions)
        {
            X = position.X;
            Y = position.Y;
            Width = dimensions.X;
            Height = dimensions.Y;
            Position = position;
            Dimensions = dimensions;
            Center = Position + (Dimensions / 2);
            Size = dimensions.X * dimensions.Y;
        }
    }

    public struct GeneratedMapDetail
    {

    }

    private static MapGenerator mapGenerator;

    public void Start()
    {
        mapGenerator = this;
        roomAreas = new List<Area>();
        occupiedLayerTiles = new Dictionary<GenerationLayer, bool[,]>();
        if (LobbyManager.self != null)
        {
            if (LobbyManager.self.serverOwner)
                CreateWorld(mapWidth, mapHeight);
        }
        else
            CreateWorld(mapWidth, mapHeight);
    }

    /// <summary>
    /// Creates a world.
    /// </summary>
    /// <param name="mapWidth">The "width" of the map when seen in a top-down perspective.</param>
    /// <param name="mapHeight">The "height" of the map when seen in a top-down perspective.</param>
    public void CreateWorld(int mapWidth, int mapHeight)
    {
        worldRand = new System.Random();

        generationDetails = new GenerationDetails(new byte[1] { Tile.Dirt }, new int[1] { 100 });
        GenerationLayer floorLayer = CreateFloorLayer(mapWidth, mapHeight);
        occupiedLayerTiles.Add(floorLayer, new bool[mapWidth, mapHeight]);
        generationDetails = new GenerationDetails(new byte[1] { Tile.Wall }, new int[1] { 100 });
        GenerationLayer lowerWallLayer = new GenerationLayer(WrapExistingLayer(generationDetails, floorLayer), 2);
        GenerationLayer upperWallLayer = new GenerationLayer(WrapExistingLayer(generationDetails, floorLayer), 3);
        worldData = new GenerationLayer[3] { floorLayer, lowerWallLayer, upperWallLayer };
        ConvertLayersTo3D(worldData);
        if (LobbyManager.self != null)
            SyncCall.SyncWorld(worldData);

        for (int i = 0; i < roomStructures.Length; i++)
        {
            InnerMapDetail detailInformation = roomStructures[i].GetComponent<InnerMapDetail>();
            for (int j = 0; j < detailInformation.expectedAmount; j++)
            {
                GenerateRoomStructure(i, detailInformation);
            }
        }

        for (int i = 0; i < outerMapDecorations.Length; i++)
        {
            MapDetail detailInformation = outerMapDecorations[i].GetComponent<MapDetail>();
            for (int j = 0; j < detailInformation.expectedAmount; j++)
            {
                GenerateOuterDecoration(i, detailInformation);
            }
        }

        Point[] spawnPoints = new Point[LobbyManager.GetAmountOfPlayersInLobby()];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPoints[i] = new Point(worldRand.Next(0, mapWidth), worldRand.Next(0, mapHeight));
        }
        SpawnPlayers(spawnPoints);
        SyncCall.SyncSpawnPoints(spawnPoints);
    }

    /// <summary>
    /// Loads a world with the given data.
    /// </summary>
    public static void LoadWorld(GenerationLayer[] layers)
    {
        mapGenerator.ConvertLayersTo3D(layers);
    }

    public static void SpawnPlayers(Point[] spawnPoints)
    {
        LobbyManager.playerObjects = new GameObject[spawnPoints.Length];
        LobbyManager.playerRagdolls = new Ragdoll[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject ragdoll = Instantiate(Resources.Load("SmollFigure") as GameObject, new Vector3(spawnPoints[i].X, 2, spawnPoints[i].Y), Quaternion.identity);
            ragdoll.transform.GetChild(0).GetComponent<Ragdoll>().controllerID = (byte)i;
            LobbyManager.playerObjects[i] = ragdoll;
            LobbyManager.playerRagdolls[i] = ragdoll.transform.GetChild(0).GetComponent<Ragdoll>();
            if (i == LobbyManager.self.clientID)
                Instantiate(Resources.Load("AttatchableCamera") as GameObject, new Vector3(0f, 0f, -12f), Quaternion.identity, ragdoll.transform.GetChild(0).transform.GetChild(0)).GetComponent<AttatchableCamera>().objectScale = 0.3f;
        }
    }

    /// <summary>
    /// Checks if the given point is out of the bounds of the map.
    /// </summary>
    /// <param name="point">The point</param>
    /// <returns>Whether or not the point is out of the bounds of the map.</returns>
    public bool CheckForOOB(Point point) => point.X < 0 || point.X >= mapWidth || point.Y < 0 || point.Y >= mapHeight;

    /// <summary>
    /// Checks if the given point is out of the bounds of the map.
    /// </summary>
    /// <param name="point">The point</param>
    /// <returns>Whether or not the point is out of the bounds of the map.</returns>
    public bool CheckForOOB(int x, int y) => x < 0 || x >= mapWidth || y < 0 || y >= mapHeight;

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

        roomAreas.Add(new Area(roomTopLeft, area, area));
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

        int amountOfRooms = worldRand.Next(minimumAmountOfRooms, maximumAmountOfRooms + 1);
        Rectangle[] rooms = new Rectangle[amountOfRooms];
        Point[] roomCenters = new Point[amountOfRooms];
        for (int i = 0; i < amountOfRooms; i++)
        {
            int roomCenterX = worldRand.Next(2 + (minimumRoomSize / 2), width - (minimumRoomSize / 2) - 2);
            int roomCenterY = worldRand.Next(2 + (maximumRoomSize / 2), height - (maximumRoomSize / 2) - 2);
            roomCenters[i] = new Point(roomCenterX, roomCenterY);
            if (i == 0)
                playerSpawnPoint = roomCenters[i];

            int roomSize = worldRand.Next(minimumRoomSize, maximumRoomSize + 1);

            bool stopRoomGeneration = false;
            rooms[i] = new Rectangle(roomCenters[i], new Point(roomSize));
            for (int j = 0; j < i; j++)
            {
                if (i != j && rooms[i].Intersects(rooms[j]))
                {
                    stopRoomGeneration = true;
                    break;       //just abort creation, let's not deal with overlaps for the funnies.
                }

                if (Vector2.Distance(roomCenters[i].ToVector2(), roomCenters[j].ToVector2()) < minimumRoomSpreadDistance)
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

            if (roomHallways[i] >= maxHallwaysPerRoom)
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

    public void GenerateRoomStructure(int index, InnerMapDetail details, int placementLayer = 0)
    {
        int padding = details.wallPadding >= roomDecorationPadding ? details.wallPadding : roomDecorationPadding;
        bool safeToGenerate = false;
        int generationAttempts = 0;
        Point spawnPoint = Point.Zero;
        while (!safeToGenerate && generationAttempts < 5)
        {
            int roomIndex = worldRand.Next(0, roomAreas.Count);
            if (roomAreas[roomIndex].X - padding - 2 < details.width || roomAreas[roomIndex].Y - padding - 2 < details.height)
            {
                generationAttempts++;
                continue;
            }

            spawnPoint = roomAreas[roomIndex].Center + new Point(worldRand.Next(-(roomAreas[roomIndex].Dimensions.X / 2) + padding, (roomAreas[roomIndex].Dimensions.X / 2) - padding + 1), worldRand.Next(-(roomAreas[roomIndex].Dimensions.Y / 2) + padding, (roomAreas[roomIndex].Dimensions.Y / 2) - padding + 1));
            safeToGenerate = true;
            for (int x = spawnPoint.X - (details.width / 2); x < spawnPoint.X + (details.width / 2); x++)
            {
                for (int y = spawnPoint.Y - (details.height / 2); y < spawnPoint.Y + (details.height / 2); y++)
                {
                    if (!TileExistsInAnyLayer(x, y) || (!details.canClip && !CheckForOOB(x, y) && occupiedLayerTiles[worldData[placementLayer]][x, y]))
                    {
                        safeToGenerate = false;
                        break;
                    }
                }
                if (!safeToGenerate)
                    break;
            }
            generationAttempts++;
        }
        if (!safeToGenerate)
            return;

        for (int x = spawnPoint.X - (details.width / 2); x < spawnPoint.X + (details.width / 2); x++)
        {
            for (int y = spawnPoint.Y - (details.height / 2); y < spawnPoint.Y + (details.height / 2); y++)
            {
                if (!CheckForOOB(x, y))
                    occupiedLayerTiles[worldData[placementLayer]][x, y] = true;
            }
        }

        GameObject newObject = Instantiate(roomStructures[index], mapContainer.transform);
        newObject.transform.position = new Vector3(spawnPoint.X, 1f + details.heightAboveBase + worldRand.Next(0, details.heightVariance + 1), spawnPoint.Y);
        newObject.transform.rotation = Quaternion.Euler(0f, details.verticalRotation + worldRand.Next(-details.verticalRotationVariance, details.verticalRotationVariance + 1), 0f);
    }

    public void GenerateHallwayStructure(int index, MapDetail details)
    {

    }

    /// <summary>
    /// Attempts to place an object of the given index in the map.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="details"></param>
    public void GenerateOuterDecoration(int index, MapDetail details, int placementLayer = 0)
    {
        Point spawnPoint = new Point(worldRand.Next((details.width / 2) + 1 - decorationPadding, mapWidth - (details.width / 2) + decorationPadding), worldRand.Next((details.height / 2) + 1 - decorationPadding, mapHeight - (details.height / 2) + decorationPadding));
        bool safeToGenerate = true;
        for (int x = spawnPoint.X - (details.width / 2); x < spawnPoint.X + (details.width / 2); x++)
        {
            for (int y = spawnPoint.Y - (details.height / 2); y < spawnPoint.Y + (details.height / 2); y++)
            {
                if ((!details.canSpawnOverTiles && TileExistsInAnyLayer(x, y)) || (!details.canClip && !CheckForOOB(x, y) && occupiedLayerTiles[worldData[placementLayer]][x, y]))
                {
                    safeToGenerate = false;
                    break;
                }
            }
            if (!safeToGenerate)
                break;
        }
        if (!safeToGenerate)
            return;

        for (int x = spawnPoint.X - (details.width / 2); x < spawnPoint.X + (details.width / 2); x++)
        {
            for (int y = spawnPoint.Y - (details.height / 2); y < spawnPoint.Y + (details.height / 2); y++)
            {
                if (!CheckForOOB(x, y))
                    occupiedLayerTiles[worldData[placementLayer]][x, y] = true;
            }
        }

        GameObject newObject = Instantiate(outerMapDecorations[index], mapContainer.transform);
        newObject.transform.position = new Vector3(spawnPoint.X, details.altitude + worldRand.Next(-details.altitudeVariance, details.altitudeVariance + 1), spawnPoint.Y);
        newObject.transform.rotation = Quaternion.Euler(0f, details.verticalRotation + worldRand.Next(-details.verticalRotationVariance, details.verticalRotationVariance + 1), 0f);
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
    public int FindClosestRoom(Point roomCenter, Point[] roomCenters, int[] roomHallwayCounts)
    {
        int closestRoomIndex = 0;

        int closestDistance = mapWidth;
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
        Texture2D dungeonResultOverviewTexture = new Texture2D(mapGenerator.mapWidth, mapGenerator.mapHeight);
        Color32[] resultData = new Color32[mapGenerator.mapWidth * mapGenerator.mapHeight];
        for (int x = 0; x < mapGenerator.mapWidth; x++)
        {
            for (int y = 0; y < mapGenerator.mapHeight; y++)
            {
                byte tileType = generationLayer.layerTiles[x, y].tileType;
                Color32 pixelColor = Color.black;
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
                if (tileType == Tile.Dirt)
                    pixelColor = Color.red;
                else if (tileType == Tile.Wall)
                    pixelColor = Color.blue;


                resultData[x + y * mapGenerator.mapHeight] = pixelColor;
            }
        }
        dungeonResultOverviewTexture.SetPixels32(resultData, 0);
        dungeonResultOverviewTexture.Apply();

        byte[] image = dungeonResultOverviewTexture.EncodeToPNG();
        string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/ProjectGoofScreenshots";
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

    /// <summary>
    /// Checks whether or not a tile exists at a given point in any layer.
    /// </summary>
    /// <returns></returns>
    private bool TileExistsInAnyLayer(int x, int y)
    {
        if (CheckForOOB(x, y))      //If it's OOB just let it be :)
            return false;

        for (int i = 0; i < worldData.Length; i++)
        {
            if (worldData[i].layerTiles[x, y].tileType != Tile.Air)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks whether or not a tile exists at a given point in any layer.
    /// </summary>
    /// <returns></returns>
    private bool TileExistsInAnyLayer(Point point)
    {
        for (int i = 0; i < worldData.Length; i++)
        {
            if (worldData[i].layerTiles[point.X, point.Y].tileType != Tile.Air)
            {
                return true;
            }    
        }
        return false;
    }

    private void ConvertLayersTo3D(GenerationLayer[] generationLayers)
    {
        /*for (int i = 0; i < generationLayers.Length; i++)
        {
            Dictionary<Point, bool> ignoreTile = new Dictionary<Point, bool>();
            for (int x = 0; x < generationLayers[i].width; x++)
            {
                for (int y = 0; y < generationLayers[i].height; y++)
                {
                    if (!ignoreTile.ContainsKey(new Point(x, y)) && generationLayers[i].layerTiles[x, y].tileType != Tile.Air)
                    {
                        Vector3 areaArea = GetSurroundingTileArea(generationLayers[i], new Point(x, y));        //The entire rect size and position of the area.
                        Vector2 areaCenter = (new Point(x, y) + new Point((int)areaArea.x / 2, (int)areaArea.z / 2)).ToVector2();
                        GameObject tile = Instantiate(GetTileTypeEquivalent(generationLayers[i].layerTiles[x, y].tileType), mapContainer.transform);
                        tile.transform.position = new Vector3(areaCenter.x, generationLayers[i].layerLevel, areaCenter.y);
                        tile.transform.localScale = new Vector3(Math.Abs(areaArea.x), areaArea.y, Math.Abs(areaArea.z));
                        Point[] includedPoints = GetSurroundingTileAreaPoints(generationLayers[i], new Point(x, y));
                        /*for (int j = 0; j < includedPoints.Length; j++)
                        {
                            if (!ignoreTile.ContainsKey(includedPoints[i]))
                                ignoreTile.Add(includedPoints[i], true);
                        }*/
        /*for (int x2 = 0; x2 < areaArea.x; x2++)
        {
            for (int y2 = 0; y2 < areaArea.y; y2++)
            {
                ignoreTile.Add(new Point(x + x2, y + y2), true);
            }
        }
        //tile.transform.position = new Vector3(x, generationLayers[i].layerLevel + 0.5f, y);
    }
}
}
}*/

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

    public Vector3 GetSurroundingTileArea(GenerationLayer generationLayer, Point startPoint)
    {
        List<Point> tilesToSearch = new List<Point>() { startPoint };
        Dictionary<Point, bool> tileScanned = new Dictionary<Point, bool>();
        List<Point> validTiles = new List<Point>();
        Point endPoint = Point.Zero;

        for (int i = 0; i < tilesToSearch.Count; i++)       //Recursion method
        {
            for (int j = 0; j < 4; j++)
            {
                Point checkPoint = tilesToSearch[i] + CheckIndexToUnitPoint(j);
                if (tileScanned.ContainsKey(checkPoint))
                    continue;

                tileScanned.Add(checkPoint, true);
                if (generationLayer.layerTiles[tilesToSearch[i].X, tilesToSearch[i].Y].tileType == generationLayer.layerTiles[checkPoint.X, checkPoint.Y].tileType)
                {
                    if (generationLayer.layerTiles[tilesToSearch[i].X, tilesToSearch[i].Y].generationID == generationLayer.layerTiles[checkPoint.X, checkPoint.Y].generationID)
                    {
                        validTiles.Add(checkPoint);
                        tilesToSearch.Add(checkPoint);
                    }
                }

                if (tilesToSearch.Count == 1)
                    endPoint = checkPoint;      //Or it might be more accurate to make a "farthest away" check
            }
            if (!tileScanned.ContainsKey(tilesToSearch[i]))
                tileScanned.Add(tilesToSearch[i], true);
            tilesToSearch.RemoveAt(i);
            i--;

            if (tilesToSearch.Count >= generationLayer.width * generationLayer.height)
                break;
        }

        Point[] tilesScanned = validTiles.ToArray();
        float farthestDist = 1f;
        for (int i = 0; i < tilesScanned.Length; i++)
        {
            float dist = Vector2.Distance(startPoint.ToVector2(), tilesScanned[i].ToVector2());
            if (dist > farthestDist)
            {
                farthestDist = dist;
                endPoint = tilesScanned[i];
            }
        }


        Vector2 area = (startPoint - endPoint).ToVector2();
        return new Vector3(area.x, 1, area.y);
    }

    public Point[] GetSurroundingTileAreaPoints(GenerationLayer generationLayer, Point startPoint)
    {
        List<Point> tilesToSearch = new List<Point>() { startPoint };
        Dictionary<Point, bool> tileScanned = new Dictionary<Point, bool>();

        for (int i = 0; i < tilesToSearch.Count; i++)       //Recursion method
        {
            for (int j = 0; j < 4; j++)
            {
                Point checkPoint = tilesToSearch[i] + CheckIndexToUnitPoint(j);
                if (tileScanned.ContainsKey(checkPoint))
                    continue;

                tileScanned.Add(checkPoint, true);
                if (generationLayer.layerTiles[tilesToSearch[i].X, tilesToSearch[i].Y].tileType == generationLayer.layerTiles[checkPoint.X, checkPoint.Y].tileType)
                    if (generationLayer.layerTiles[tilesToSearch[i].X, tilesToSearch[i].Y].generationID == generationLayer.layerTiles[checkPoint.X, checkPoint.Y].generationID)
                        tilesToSearch.Add(checkPoint);
            }
            if (!tileScanned.ContainsKey(tilesToSearch[i]))
                tileScanned.Add(tilesToSearch[i], true);
            tilesToSearch.RemoveAt(i);
            i--;

            if (tilesToSearch.Count >= generationLayer.width * generationLayer.height)
                break;
        }

        return tileScanned.Keys.ToArray();
    }

    public Point CheckIndexToUnitPoint(int checkNumber)
    {
        if (checkNumber == 0)
            return Point.Up;
        else if (checkNumber == 1)
            return Point.Right;
        else if (checkNumber == 2)
            return Point.Down;
        else
            return Point.Left;
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