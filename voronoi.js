// link - https://tech.io/playgrounds/243/voronoi-diagrams/lets-get-your-hands-dirty
// Create the Voronoi areas

function fillVoronoi (tiles, players) {
    /*
      Modify the `site` property of *each* tile to be a *reference* to one of the players
      given as parameter. The tile must be in that player's Voronoi site.
      ex:
        tiles[0].site = players[0];
        tiles[1].site = players[1];
    */
    var minDistance = 0;
  
    for(var t in tiles) {
      var tile = tiles[t];
      
      minDistance = distance(tile, players[0]);
      minDistancePlayer = 0;
      
      for (var i = 1; i < 4; i++)
      {
          var temp = distance(tile, players[i]);
          if (temp < minDistance)
          {
              minDistance = temp;
              minDistancePlayer = i;
          }
      }
  
      tile.site = player[minDistancePlayer];
      // TODO: Select the nearest player, assigning it to tile.site
    }
  }
  
  function distance(pointA, pointB) {
      var dx = pointA.x - pointB.x;
      var dy = pointA.y - pointB.y;
      return Math.abs(dx) + Math.abs(dy);
    // TODO: Implement the manhattan distance function
    // Both tiles and players have a `x` and a `y` property
  }