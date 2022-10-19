using System.Collections;
using System.Text;
using Newtonsoft.Json;

public static class PolylineUtility
{
    public static string EncodePolyline(IEnumerable<Coordinate> coordinates)
    {
        //https://developers.google.com/maps/documentation/utilities/polylinealgorithm
        StringBuilder resultBuilder = new();
        Coordinate[] coordsArray = coordinates.Where(c => c.Latitude != 0 && c.Longitude != 0).ToArray();
        for (int i = 0; i < coordsArray.Length; i++)
        {
            Coordinate coordinate = coordsArray[i];
            if (i == 0)
            {
                resultBuilder.Append(EncodeValue(coordinate.Latitude));
                resultBuilder.Append(EncodeValue(coordinate.Longitude));
            }
            else
            {
                var previousCoordinate = coordsArray[i - 1];
                decimal latDiff = coordinate.Latitude - previousCoordinate.Latitude;
                decimal lngDiff = coordinate.Longitude - previousCoordinate.Longitude;
                if (Math.Abs(latDiff) < 0.00001M || Math.Abs(lngDiff) <= 0.00001M)
                {
                    continue;
                }
                resultBuilder.Append(EncodeValue(latDiff));
                resultBuilder.Append(EncodeValue(lngDiff));
            }
        }
        string result = resultBuilder.ToString();
        return result;
    }

    private static string EncodeValue(decimal value)
    {
        int scaled = (int)Math.Round(value * 100000);
        int shifted = scaled << 1;
        int inverted = shifted > 0 ? shifted : ~shifted;
        int[] chunks = PruneEnd(GetChunks(inverted)).ToArray();
        IEnumerable<int> chainedChunks = ChainChunks(chunks);
        string result = new(chainedChunks.Select(ch => (char)(ch + 63)).ToArray());
        return result;
    }

    private static IEnumerable<int> GetChunks(int input)
    {
        int counter = 0;
        while (true)
        {
            uint mask = 0b00011111;
            uint chunk = (uint)input & mask;
            //Having a zero in the last chunk throws off the algorithm, so just don't send the last 
            //chunk if it's zero
            if (counter++ == 4)
            {
                if (chunk != 0)
                {
                    yield return (int)chunk;
                }
                yield break;
            }
            yield return (int)chunk;
            input >>= 5;
        }
    }


    private static IEnumerable<int> PruneEnd(IEnumerable<int> chunks)
    {
        List<int> resultsReversed = new();
        bool nonZeroFound = false;
        foreach (int chunk in chunks.Reverse())
        {
            if (chunk != 0 || nonZeroFound)
            {
                nonZeroFound = true;
                resultsReversed.Add(chunk);
            }
        }
        return resultsReversed.AsEnumerable().Reverse();
    }

    private static IEnumerable<int> ChainChunks(IEnumerable<int> chunks)
    {
        int length = chunks.Count();
        for (int i = 0; i < length - 1; i++)
        { //All but the last one
            yield return chunks.ElementAt(i) | 0x20;
        }
        yield return chunks.Last();
    }


    private static void Print<T>(string label, T value)
    {
        Console.WriteLine($"{label,-16}{value}");
    }

    private static void PrintBinary(string label, int value)
    {
        byte[] bytes = GetBitsAsBytes(value);
        string output = PrintByteArray(bytes);
        Print(label, output);
    }

    private static byte[] GetBitsAsBytes(int value)
    {
        int[] intArray = new[] { value };
        BitArray bitArray = new(intArray);
        byte[] byteArray = new byte[bitArray.Length];
        for (int i = 0; i < bitArray.Length; i++)
        {
            byteArray[bitArray.Length - (i + 1)] = (bool)bitArray[i] ? (byte)1 : (byte)0;
        }
        return byteArray;
    }

    private static string PrintByteArray(byte[] bytes)
    {
        int bitCounter = 0;
        StringBuilder sb = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            sb.Append(bytes[i]);
            if (++bitCounter == 8)
            {
                sb.Append(' ');
                bitCounter = 0;
            }
        }
        return sb.ToString();
    }

}