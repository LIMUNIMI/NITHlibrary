using NITHlibrary.Nith.Internals;

/// <summary>
/// Provides methods for parsing NITH status codes and fields from the sensor input into their corresponding enum values.
/// </summary>
internal static class NithParsers
{
    /// <summary>
    /// Parses a given string code into a <see cref="NithStatusCodes"/> enum value.
    /// Returns <see cref="NithStatusCodes.NaC"/> if parsing fails.
    /// </summary>
    /// <param name="code">The string representation of the status code to parse.</param>
    /// <returns>A <see cref="NithStatusCodes"/> enum value corresponding to the given code, or <see cref="NithStatusCodes.NaC"/> if parsing fails.</returns>
    public static NithStatusCodes ParseStatusCode(string code)
    {
        var ret = NithStatusCodes.NaC;
        try
        {
            ret = (NithStatusCodes)Enum.Parse(typeof(NithStatusCodes), code);
        }
        catch
        {

        }
        return ret;
    }

    /// <summary>
    /// Parses a given string field into a <see cref="NithParameters"/> enum value.
    /// Returns <see cref="NithParameters.NaP"/> if parsing fails.
    /// </summary>
    /// <param name="field">The string representation of the field to parse.</param>
    /// <returns>A <see cref="NithParameters"/> enum value corresponding to the given field, or <see cref="NithParameters.NaP"/> if parsing fails.</returns>
    public static NithParameters ParseField(string field)
    {
        var ret = NithParameters.NaP;
        try
        {
            ret = (NithParameters)Enum.Parse(typeof(NithParameters), field);
        }
        catch
        {

        }
        return ret;
    }
}