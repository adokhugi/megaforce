#include <stdio.h>
#include "shfo2_map.h"

void main()
{
  FILE *file;

  file = fopen ("shfo2_map.map", "wb");
  // map size
  fputc (32, file);
  fputc (30, file);
  // map data
  fputs ("MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM", file);
  fputs ("MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM", file);
  fputs ("MMMMMMMMMMHHHHHHHMMMMMMMMFFFFMMM", file);
  fputs ("MMMMMMMMHHHHHHHFFFFMMMMMMFFFFFMM", file);
  fputs ("MMMMMMMMHHHHHHFFGGFFMMMMFFPGFFMM", file);
  fputs ("MMMMMMMMHHHHHFFFGFFFHHHFFGPGGFMM", file);
  fputs ("MMMMMMHHHHFFHHFGGGFFFFFFGGPFGFHM", file);
  fputs ("MMMMMMHHHHHFFGGGFGGGFFFPPPPGFFHM", file);
  fputs ("MMMMMMHHHHHFGGGGFGGGFFPPGGGFFFHM", file);
  fputs ("MMMMHHHFHHHHGGSSFFFGGGPGFFHHHHHM", file);
  fputs ("MMMMHFFFHHHHSSSSSSFFPPPFHHHHHHHM", file);
  fputs ("MMMMHHFFHHHHSSSSGGPPPFFHHFFHHHMM", file);
  fputs ("MMMHHFFFFFHSSSSGGGPFFFGFFFFHHMMM", file);
  fputs ("MMHHHFFFFFFSSSSGGGPGGGGGFFHHMMMM", file);
  fputs ("MMHHFFFFFFSSSSSGGPPGGGGGFHHHMMMM", file);
  fputs ("FFFFFFFFFHSSSFGGGPFGGFFFFHHMMMMM", file);
  fputs ("GGGFFFFFGHSSFFFFGPGGFGFFFHHMMMMM", file);
  fputs ("GGGGFFGGGSSSFFFFFPGGFGGGFHHMMMMM", file);
  fputs ("FGGGGGGSSSSHGGGFFPGGFGGGFHMMMMMM", file);
  fputs ("FFGGGGGFFFFHHHGFFPGGGGGHHHMMMMMM", file);
  fputs ("FFFFGGGFFFFFPPPPPPFGGGHHHMMMMMMM", file);
  fputs ("FFFFGPPPPPPPPHHGGFFGGFHHMMMMMMMM", file);
  fputs ("MMFFFGGFFFFGGFFGGFFGGFHHMMMMMMMM", file);
  fputs ("MMMMFFFFFFFFFGGGFFFFFFFHMMMMMMMM", file);
  fputs ("MMMMMMFFFFFFFFFFFFFFFFHHMMMMMMMM", file);
  fputs ("MMMMMMMMFFMMFFFFFFFFFHHHHMMMMMMM", file);
  fputs ("FFMMMMMMMMMMMMMMHHHFFHHHHMMMMMMM", file);
  fputs ("FFFFMMMMMMMMMMMMHHHHHHHHHMMMMMMM", file);
  fputs ("FFFFFFMMMMMMMMMMMMMMMMMMMMMMMMMM", file);
  fputs ("FFFFFFFMMMMMMMMMMMMMMMMMMMMMMMMM", file);
  // starting starting of the map
  fputc (0, file);
  fputc (11, file);
  // starting position of party member 0
  fputc (4, file);
  fputc (18, file);
  // starting position of party member 1
  fputc (3, file);
  fputc (17, file);
  // starting position of party member 2
  fputc (3, file);
  fputc (18, file);
  // starting position of party member 3
  fputc (2, file);
  fputc (18, file);
  // starting position of party member 4
  fputc (3, file);
  fputc (19, file);
  // starting position of party member 5
  fputc (2, file);
  fputc (17, file);
  // starting position of party member 6
  fputc (2, file);
  fputc (19, file);
  // starting position of party member 7
  fputc (2, file);
  fputc (16, file);
  // starting position of party member 8
  fputc (3, file);
  fputc (16, file);
  // starting position of party member 9
  fputc (1, file);
  fputc (16, file);
  // starting position of party member 10
  fputc (1, file);
  fputc (17, file);
  // starting position of party member 11
  fputc (1, file);
  fputc (18, file);
  // ID and starting position of enemy member 0
  fputc (DarkSniper, file);
  fputc (8, file);
  fputc (24, file);
  // ID and starting position of enemy member 1
  fputc (DarkSniper, file);
  fputc (18, file);
  fputc (13, file);
  // ID and starting position of enemy member 2
  fputc (DarkSniper, file);
  fputc (25, file);
  fputc (5, file);
  // ID and starting position of enemy member 3
  fputc (LesserDemon, file);
  fputc (8, file);
  fputc (8, file);
  // ID and starting position of enemy member 4
  fputc (LesserDemon, file);
  fputc (14, file);
  fputc (3, file);
  // ID and starting position of enemy member 5
  fputc (LesserDemon, file);
  fputc (21, file);
  fputc (26, file);
  // ID and starting position of enemy member 6
  fputc (LesserDemon, file);
  fputc (22, file);
  fputc (25, file);
  // ID and starting position of enemy member 7
  fputc (LesserDemon, file);
  fputc (28, file);
  fputc (10, file);
  // ID and starting position of enemy member 8
  fputc (Skeleton, file);
  fputc (9, file);
  fputc (25, file);
  // ID and starting position of enemy member 9
  fputc (Skeleton, file);
  fputc (12, file);
  fputc (21, file);
  // ID and starting position of enemy member 10
  fputc (Skeleton, file);
  fputc (19, file);
  fputc (20, file);
  // ID and starting position of enemy member 11
  fputc (Skeleton, file);
  fputc (18, file);
  fputc (18, file);
  // ID and starting position of enemy member 12
  fputc (Skeleton, file);
  fputc (22, file);
  fputc (14, file);
  // ID and starting position of enemy member 13
  fputc (Skeleton, file);
  fputc (25, file);
  fputc (8, file);
  // ID and starting position of enemy member 14
  fputc (DarkBishop, file);
  fputc (19, file);
  fputc (19, file);
  // ID and starting position of enemy member 15
  fputc (DarkBishop, file);
  fputc (26, file);
  fputc (7, file);
  // stop marker
  fputc (255, file);
  fclose (file);
}