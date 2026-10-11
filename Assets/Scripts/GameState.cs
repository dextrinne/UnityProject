public static class GameState
{
    public static bool IsGameFinished = false;

    /// <summary>
    /// Сбрасывает состояние игры, устанавливая флаг завершения в false
    /// </summary>
    public static void Reset()
    {
        IsGameFinished = false;
    }
}