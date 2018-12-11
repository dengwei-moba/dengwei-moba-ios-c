namespace TrueSync
{

    /**
    *  @brief Represents a ray with origin and direction. 
     *  表示具有原点和方向的射线。 
    **/
    public class TSRay
	{
		public TSVector direction;
		public TSVector origin;

		public TSRay (TSVector origin, TSVector direction)
		{
			this.origin = origin;
			this.direction = direction;
		}

	}
}

