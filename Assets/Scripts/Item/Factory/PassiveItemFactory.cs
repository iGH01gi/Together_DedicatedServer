using Item.Product;

namespace Item.Factory
{
    public abstract class PassiveItemFactory
    {
        public IPassiveItem CreatePassiveItem()
        {
            IPassiveItem product = CreateProduct(); //서브 클래스에서 구체화한 팩토리 메서드 실행
            product.Setting(); //.. 이밖의 객체 생성에 가미할 로직 실행
            return product; //완성된 제품을 반환
        }

        abstract protected IPassiveItem CreateProduct();
    }
}