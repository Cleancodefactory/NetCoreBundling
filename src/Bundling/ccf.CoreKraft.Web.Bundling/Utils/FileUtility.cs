using System.Linq;

namespace ccf.CoreKraft.Web.Bundling.Utils
{
    internal static class FileUtility
    {
        internal enum EStartPoint
        {
            FromStart,
            FromEnd
        }

        internal static string RemoveFirstOccurenceSpecialCharacters(string fileName, EStartPoint startPoint, char[] specialCharacters)
        {
            char[] buffer = new char[fileName.Length];
            int idx = 0;
            bool done = false;
            switch (startPoint)
            {
                case EStartPoint.FromStart:
                    {
                        for (int i = 0; i < fileName.Length; i++)
                        {
                            if (!done && specialCharacters.Contains(fileName[i]))
                            {
                                //do nothing
                            }
                            else
                            {
                                done = true;
                                buffer[idx++] = fileName[i];
                            }
                        }
                        break;
                    }
                case EStartPoint.FromEnd:
                    {
                        for (int i = fileName.Length - 1; i > 0; i--)
                        {
                            if (!done && specialCharacters.Contains(fileName[i]))
                            {
                                //do nothing
                            }
                            else
                            {
                                done = true;
                                buffer[idx++] = fileName[i];
                                buffer.Reverse();
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
            return new string(buffer, 0, idx);
        }
    }
}
