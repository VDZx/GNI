using System;
using System.Collections.Generic;
using System.Text;

/*
 * GenericNetplayImplementation - A networking library for C# (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/GNI>.
 */

namespace GenericNetplayImplementation
{
    public static class GNIGeneral
    {
        public static int GetLengthLength(GNIDataType type)
        {
            switch (type)
            {
                case GNIDataType.None: return 0;
                case GNIDataType.Short: return 0;
                case GNIDataType.String: return 2;
                case GNIDataType.ByteArray: return 4;
            }
            return 1;
        }
    }
}
