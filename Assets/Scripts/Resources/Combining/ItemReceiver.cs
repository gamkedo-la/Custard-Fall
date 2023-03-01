public interface ItemReceiver
{
    bool CanReceiveItem(PlaceableItem material);
    bool PreviewReceiveItem(PlaceableItem material);
    bool ReceiveItem(PlaceableItem placeModeItemReference);
    void OnPreviewLeave();
}